// Just enough of Survival Log's web UI for Full Queue's page.js to run in Jint: the root page holding the HUD pages in
// iframes, and CoreUI1 with its queue bar. The bar is laid out as the game's CSS lays it out (slots 32.2px wide and
// 2.1px apart inside 2.8px of padding and a 1px border), and the mod's long class caps it at five slots, as page.js's
// style does. CoreUI1's own message listener is the game's, which cuts the queue to five, and the page redraws its slots
// the way Vue does: one slot element per list index, kept while the index exists, after every listener has run.
// Test code only; nothing here ships.
var window = this;

var warnings = [];
var console = { warn: function (text) { warnings.push(String(text)); } };

// What the script costs the browser, counted across every page: element searches (querySelector,
// querySelectorAll, getElementById), box reads that make the browser lay the page out, and looks at an iframe in the
// root page's frame list. The page itself counts its evals and JSON parses. resetCounts() zeroes them all.
var searches = 0;
var layoutReads = 0;
var frameLooks = 0;

function resetCounts(page) {
  searches = 0;
  layoutReads = 0;
  frameLooks = 0;
  page.evals = 0;
  page.parses = 0;
}

var intervals = [];

function setInterval(callback) {
  intervals.push(callback);

  return intervals.length;
}

// One run of every interval the script set, which is how its poll runs. Vue draws whatever the poll changed.
function poll() {
  intervals.slice().forEach(function (callback) { callback(); });
  openPages.forEach(draw);
}

var openPages = [];

var SLOT_PX = 32.2;
var GAP_PX = 2.1;
var PAD_PX = 2.8;
var BORDER_PX = 1;
var BAR_LEFT_PX = 120;
var GAME_SLOTS = 5;

// CoreUI1's body zoom from the game's CSS. Boxes are in screen pixels, so they are this much larger than the page
// pixels of widths and scroll positions.
var ZOOM = 1.3;

var FakeElement = class {
  constructor(tagName, className) {
    var element = this;

    this.tagName = tagName;
    this.id = '';
    this.parentElement = null;
    this.ownerDocument = null;
    this.children = [];
    this.textContent = '';
    this.attrs = {};
    this.classes = className ? className.split(' ') : [];
    this.style = { pointerEvents: '' };
    this.listeners = {};
    this.classList = {
      contains: function (name) { return element.classes.indexOf(name) >= 0; },
      toggle: function (name, force) {
        var isOn = force === undefined ? element.classes.indexOf(name) < 0 : !!force;

        element.classes = element.classes.filter(function (c) { return c !== name; });

        if (isOn) element.classes.push(name);

        return isOn;
      }
    };
  }

  get className() { return this.classes.join(' '); }

  set className(value) { this.classes = value ? String(value).split(' ').filter(Boolean) : []; }

  // In a document: its topmost ancestor is that document's root element.
  get isConnected() {
    var top = this;

    while (top.parentElement) top = top.parentElement;

    return !!top.ownerDocument && top === top.ownerDocument.documentElement;
  }

  hasAttribute(name) { return Object.prototype.hasOwnProperty.call(this.attrs, name); }

  getAttribute(name) { return this.hasAttribute(name) ? this.attrs[name] : null; }

  setAttribute(name, value) { this.attrs[name] = String(value); }

  removeAttribute(name) { delete this.attrs[name]; }

  appendChild(child) {
    child.parentElement = this;
    child.ownerDocument = this.ownerDocument;
    this.children.push(child);

    return child;
  }

  removeChild(child) {
    this.children = this.children.filter(function (c) { return c !== child; });
    child.parentElement = null;

    return child;
  }

  addEventListener(type, listener) {
    (this.listeners[type] = this.listeners[type] || []).push(listener);
  }

  dispatch(type, event) {
    var element = this;

    event.currentTarget = element;
    (this.listeners[type] || []).slice().forEach(function (listener) { listener.call(element, event); });

    return event;
  }

  getBoundingClientRect() {
    layoutReads++;

    return { left: 0, right: 0 };
  }

  querySelectorAll(selector) {
    searches++;

    return findAll(this, selector);
  }

  querySelector(selector) {
    searches++;

    return findAll(this, selector)[0] || null;
  }
};

// The game's .queue-container. Its width follows its slots; with the mod's long class and style it stops at five.
var FakeBar = class extends FakeElement {
  constructor() {
    super('div', 'queue-container');
    this.scrollLeft = 0;
    this.clientLeft = BORDER_PX;
  }

  get slots() { return this.children.filter(function (child) { return child.classList.contains('queue-slot'); }); }

  get isCapped() {
    return this.classList.contains('slfq-long') && !!this.ownerDocument && findById(this.ownerDocument, 'slfq-style') !== null;
  }

  get scrollWidth() { return 2 * PAD_PX + rowWidth(this.slots.length); }

  get clientWidth() {
    var shown = this.isCapped ? Math.min(this.slots.length, GAME_SLOTS) : this.slots.length;

    return 2 * PAD_PX + rowWidth(shown);
  }

  get offsetWidth() { return this.clientWidth + 2 * BORDER_PX; }

  getBoundingClientRect() {
    layoutReads++;

    return screenBox(BAR_LEFT_PX, this.offsetWidth);
  }
};

// One .queue-slot, drawn for the order at its index in the page's list.
var FakeSlot = class extends FakeElement {
  constructor() {
    super('div', 'queue-slot');
    this.order = null;
    this.setAttribute('data-interactive', '');
  }

  draw(order) {
    var isFilled = !!(order.icon || order.name);

    this.order = order;
    this.classes = ['queue-slot'];

    if (isFilled) this.classes.push('filled');
    if (isFilled && order.isCancellable) this.classes.push('cancellable');
  }

  getBoundingClientRect() {
    var bar = this.parentElement;
    var left = BAR_LEFT_PX + BORDER_PX + PAD_PX + bar.slots.indexOf(this) * (SLOT_PX + GAP_PX) - bar.scrollLeft;

    layoutReads++;

    return screenBox(left, SLOT_PX);
  }
};

// A box at this page position and width, as the browser reports it: in screen pixels.
function screenBox(left, width) { return { left: left * ZOOM, right: (left + width) * ZOOM, width: width * ZOOM }; }

function rowWidth(slots) { return slots > 0 ? slots * SLOT_PX + (slots - 1) * GAP_PX : 0; }

// One simple selector: a tag, a class or an id.
function matches(element, selector) {
  if (selector.charAt(0) === '.') return element.classList.contains(selector.slice(1));
  if (selector.charAt(0) === '#') return element.id === selector.slice(1);
  if (/^[a-z]+$/.test(selector)) return element.tagName === selector;

  throw new Error('the fake page does not know the selector "' + selector + '"');
}

// The fake's own lookups, not counted as the script's searches.
function findAll(root, selector) {
  return descendantsOf(root).filter(function (element) { return matches(element, selector); });
}

function findById(doc, id) {
  return descendantsOf(doc.documentElement).filter(function (element) { return element.id === id; })[0] || null;
}

function descendantsOf(root) {
  var found = [];

  root.children.forEach(function walk(child) {
    found.push(child);
    child.children.forEach(walk);
  });

  return found;
}

function makeDocument() {
  var root = new FakeElement('html');
  var doc = {
    readyState: 'complete',
    defaultView: null,
    documentElement: root,
    createElement: function (tagName) { return new FakeElement(tagName); },
    getElementById: function (id) {
      searches++;

      return findById(doc, id);
    },
    querySelector: function (selector) { return root.querySelector(selector); },
    querySelectorAll: function (selector) { return root.querySelectorAll(selector); },

    // The browser's live list: it always holds the elements in the document now.
    getElementsByTagName: function (tagName) {
      return {
        get length() { return findAll(root, tagName).length; },
        item: function (index) {
          frameLooks++;

          return findAll(root, tagName)[index] || null;
        }
      };
    }
  };

  root.ownerDocument = doc;
  doc.head = root.appendChild(new FakeElement('head'));
  doc.body = root.appendChild(new FakeElement('body'));

  return doc;
}

// The root page the plugin runs its scripts in.
var document = makeDocument();

// CoreUI1 in an iframe of the root page, its queue bar empty until the game sends the queue. A page in a browser that
// runs a window's listeners in the order they were added, capture or not, has capturesFirst set to false.
function openCoreUI1() {
  var frame = document.body.appendChild(new FakeElement('iframe'));
  var doc = makeDocument();
  var page = {
    document: doc,
    JSON: {
      parse: function (text) {
        page.parses++;

        return JSON.parse(text);
      },
      stringify: function (value) { return JSON.stringify(value); }
    },
    evals: 0,
    parses: 0,
    state: { queueList: [], currentAction: { name: '' } },
    capturesFirst: true,
    windowListeners: [],
    frames: [],
    slotsMade: 0,
    cancelled: [],
    addEventListener: function (type, listener, capture) {
      page.windowListeners.push({ type: type, listener: listener, capture: capture === true });
    },
    requestAnimationFrame: function (callback) {
      page.frames.push(callback);

      return page.frames.length;
    },
    eval: function (name) {
      page.evals++;

      if (name === 'state') return page.state;

      throw new ReferenceError(name + ' is not defined');
    }
  };

  doc.defaultView = page;
  frame.src = 'https://webui.local/UI/CoreUI1/CoreUI1.html';
  frame.contentWindow = page;
  frame.contentDocument = doc;
  page.frame = frame;
  page.bar = doc.body.appendChild(new FakeElement('div', 'queue-flow-wrapper')).appendChild(new FakeBar());
  page.addEventListener('message', gameListener(page));
  openPages.push(page);

  return page;
}

// The game dropping CoreUI1's iframe, as it does when it reloads the page.
function closeCoreUI1(page) {
  document.body.removeChild(page.frame);
  openPages = openPages.filter(function (open) { return open !== page; });
}

// CoreUI1's message listener and its WebUI_CoreUI1_ActionMsg handler, cut down to what the queue needs.
function gameListener(page) {
  return function (event) {
    var message = event.data;

    if (!message || !message.type || message.type !== 'WebUI_CoreUI1_ActionMsg') return;

    var data = typeof message.data === 'string' && message.data ? JSON.parse(message.data) : (message.data || {});

    if (data.name !== undefined) page.state.currentAction.name = data.name;

    if (data.queueJson) {
      var parsed = JSON.parse(data.queueJson);

      page.state.queueList = [];

      for (var i = 0; i < GAME_SLOTS; i++) {
        page.state.queueList.push(parsed[i] ? parsed[i] : { icon: '', name: '', showIcon: false, isCancellable: false, isAiDriven: false, parentId: 0 });
      }
    }
  };
}

// The game's update reaching the page: every message listener of the window in the browser's order, with Vue drawing
// the list after each, as the browser's microtasks let it. update: the fields of WebUI_CoreUI1_ActionMsg.
function gameSends(page, update) {
  var event = { data: { type: 'WebUI_CoreUI1_ActionMsg', data: JSON.stringify(update), sourcePageId: 'CoreUI1' } };
  var listeners = page.windowListeners.filter(function (entry) { return entry.type === 'message'; });
  var ordered = page.capturesFirst
    ? listeners.filter(function (entry) { return entry.capture; }).concat(listeners.filter(function (entry) { return !entry.capture; }))
    : listeners;

  ordered.forEach(function (entry) {
    entry.listener.call(page, event);
    draw(page);
  });
}

function sendQueue(page, queueText) { gameSends(page, { queueJson: queueText }); }

function listenersOf(page, type) {
  return page.windowListeners.filter(function (entry) { return entry.type === type; });
}

// The browser window changing size, which moves the bar without any update from the game.
function resize(page) {
  listenersOf(page, 'resize').forEach(function (entry) { entry.listener.call(page, {}); });
}

// Vue keys the slots by index: a slot element stays while its index does and is drawn with whatever order is there now.
// slotsMade counts the slot elements made, since a new one plays the game's appear animation.
function draw(page) {
  var list = page.state.queueList;
  var slots = page.bar.slots;

  for (var i = list.length; i < slots.length; i++) page.bar.removeChild(slots[i]);

  for (var j = 0; j < list.length; j++) {
    var slot = slots[j];

    if (!slot) {
      slot = page.bar.appendChild(new FakeSlot());
      page.slotsMade++;
    }

    slot.draw(list[j]);
  }

  // A browser keeps the scroll position inside the bar's new content, without telling the page.
  page.bar.scrollLeft = Math.min(page.bar.scrollLeft, Math.max(0, page.bar.scrollWidth - page.bar.clientWidth));
}

// The next animation frame of the page.
function nextFrame(page) {
  var callbacks = page.frames;

  page.frames = [];
  callbacks.forEach(function (callback) { callback(); });
}

// The mouse wheel over the bar. A browser fires scroll after the bar's scroll position changes. Returns whether the
// script kept the wheel from the page.
function wheel(page, deltaY, deltaMode) {
  var event = {
    deltaX: 0,
    deltaY: deltaY,
    deltaMode: deltaMode || 0,
    defaultPrevented: false,
    preventDefault: function () { this.defaultPrevented = true; }
  };
  var before = page.bar.scrollLeft;

  page.bar.dispatch('wheel', event);

  if (page.bar.scrollLeft !== before) page.bar.dispatch('scroll', {});

  return event.defaultPrevented;
}

// The bar scrolled by something other than the script's wheel handler, as a browser does when it keeps the scroll
// position inside shrunk content.
function scrollTo(page, px) {
  page.bar.scrollLeft = px;
  page.bar.dispatch('scroll', {});
}

function slotPitch() { return SLOT_PX + GAP_PX; }

// Whether the bar is scrolled by a whole number of slots.
function isScrolledByWholeSlots(page) {
  var slots = page.bar.scrollLeft / slotPitch();

  return Math.abs(slots - Math.round(slots)) < 1e-6;
}

// The game's click-through collects the full box of every element marked data-interactive whose pointer events are
// on, clipped or not, and the world gets no click inside any of them. Lists the slots whose box it would collect past
// the bar's visible inside, such as "2,7".
function strayClickAreas(page) {
  var left = page.bar.getBoundingClientRect().left + page.bar.clientLeft * ZOOM;
  var right = left + page.bar.clientWidth * ZOOM;

  return page.bar.slots
    .map(function (slot, index) {
      var box = slot.getBoundingClientRect();
      var isCollected = slot.hasAttribute('data-interactive') && slot.style.pointerEvents !== 'none';

      return isCollected && (box.left < left || box.right > right) ? String(index) : null;
    })
    .filter(function (index) { return index !== null; })
    .join(',');
}

// A click where the slot is drawn: the game's handler cancels a filled, cancellable slot by its order's parent id. A slot
// whose pointer events are off is not hit. Returns the parent id sent, or '' when nothing was.
function clickSlot(page, index) {
  var slot = page.bar.slots[index];

  if (!slot || slot.style.pointerEvents === 'none') return '';
  if (!slot.classList.contains('cancellable')) return '';

  page.cancelled.push(slot.order.parentId);

  return String(slot.order.parentId);
}

function shownNames(page) {
  return page.bar.slots.map(function (slot) { return slot.order.name; }).join(',');
}

function slotPointerEvents(page) {
  return page.bar.slots.map(function (slot) { return slot.style.pointerEvents || 'auto'; }).join(',');
}

// Every count shown on the bar as index:side:text, such as "4:after:+3".
function counts(page) {
  var found = [];

  page.bar.slots.forEach(function (slot, index) {
    if (slot.hasAttribute('data-slfq-before')) found.push(index + ':before:' + slot.getAttribute('data-slfq-before'));
    if (slot.hasAttribute('data-slfq-after')) found.push(index + ':after:' + slot.getAttribute('data-slfq-after'));
  });

  return found.join(',');
}

function styleOn(page) { return page.document.getElementById('slfq-style'); }

// ===== The countdown screen =====

// What the root page posts to the plugin through the web view.
var posted = [];

window.vuplex = { postMessage: function (message) { posted.push(String(message)); } };

// The countdown bar's layout from page.js's style, unzoomed: CoreUI0 has no body zoom.
var PRE_SLOT_PX = 41.86;
var PRE_GAP_PX = 2.73;
var PRE_PAD_PX = 3.64;
var PRE_LEFT_PX = 30;

function preRowWidth(slots) { return slots > 0 ? slots * PRE_SLOT_PX + (slots - 1) * PRE_GAP_PX : 0; }

// An element of CoreUI0. The script's bar (class slfq-pre) is laid out as page.js's style lays it out, capped at five
// slots once it is long and the style is on the page; its slots sit in a row inside it.
var FakePreElement = class extends FakeElement {
  constructor(tagName) {
    super(tagName);
    this.scrollLeft = 0;
    this.clientLeft = BORDER_PX;
  }

  get slots() { return this.children.filter(function (child) { return child.classList.contains('queue-slot'); }); }

  get isCapped() {
    return this.classList.contains('slfq-long') && !!this.ownerDocument && findById(this.ownerDocument, 'slfq-pre-style') !== null;
  }

  get scrollWidth() { return 2 * PRE_PAD_PX + preRowWidth(this.slots.length); }

  get clientWidth() { return 2 * PRE_PAD_PX + preRowWidth(this.isCapped ? Math.min(this.slots.length, GAME_SLOTS) : this.slots.length); }

  get offsetWidth() { return this.clientWidth + 2 * BORDER_PX; }

  getBoundingClientRect() {
    layoutReads++;

    if (this.classList.contains('slfq-pre')) return { left: PRE_LEFT_PX, right: PRE_LEFT_PX + this.offsetWidth, width: this.offsetWidth };

    var bar = this.parentElement;

    if (!this.classList.contains('queue-slot') || !bar) return { left: 0, right: 0, width: 0 };

    var left = PRE_LEFT_PX + BORDER_PX + PRE_PAD_PX + bar.slots.indexOf(this) * (PRE_SLOT_PX + PRE_GAP_PX) - bar.scrollLeft;

    return { left: left, right: left + PRE_SLOT_PX, width: PRE_SLOT_PX };
  }
};

// CoreUI0, the countdown screen, in an iframe of the root page: the bottom left corner's column of tools and buttons,
// and the status tags beside them.
function openCoreUI0() {
  var frame = document.body.appendChild(new FakeElement('iframe'));
  var doc = makeDocument();
  var page = {
    document: doc,
    evals: 0,
    parses: 0,
    frames: [],
    requestAnimationFrame: function (callback) {
      page.frames.push(callback);

      return page.frames.length;
    }
  };

  doc.createElement = function (tagName) { return new FakePreElement(tagName); };
  doc.defaultView = page;
  frame.src = 'https://webui.local/UI/CoreUI0/CoreUI0.html';
  frame.contentWindow = page;
  frame.contentDocument = doc;
  page.frame = frame;
  page.corner = doc.body.appendChild(new FakeElement('div', 'bottom-left'));
  page.corner.appendChild(new FakeElement('div', 'vertical-tools'));
  page.corner.appendChild(new FakeElement('div', 'action-row'));
  page.buffs = doc.body.appendChild(new FakeElement('div', 'buff-container'));

  return page;
}

function closeCoreUI0(page) { document.body.removeChild(page.frame); }

// The script's countdown bar, found without counting as the script's search.
function preBar(page) { return findAll(page.document.documentElement, '.slfq-pre')[0] || null; }

// What each slot of the countdown bar shows: its icon, its name, or nothing.
function preShown(page) {
  return preBar(page).slots.map(function (slot) {
    var content = slot.children[0];

    if (!content) return '';

    return content.tagName === 'img' ? content.src : content.textContent;
  }).join(',');
}

// The classes of the corner's children, in order, such as "vertical-tools,action-row,slfq-pre".
function cornerOrder(page) { return page.corner.children.map(function (child) { return child.classes[0]; }).join(','); }

// A click on the countdown bar's slot where it is drawn. A slot whose pointer events are off is not hit.
function clickPreSlot(page, index) {
  var slot = preBar(page).slots[index];

  posted = [];

  if (slot && slot.style.pointerEvents !== 'none') slot.dispatch('click', {});

  return posted.join(';');
}

function preCounts(page) {
  var found = [];

  preBar(page).slots.forEach(function (slot, index) {
    if (slot.hasAttribute('data-slfq-before')) found.push(index + ':before:' + slot.getAttribute('data-slfq-before'));
    if (slot.hasAttribute('data-slfq-after')) found.push(index + ':after:' + slot.getAttribute('data-slfq-after'));
  });

  return found.join(',');
}

function preStyleOn(page) { return findById(page.document, 'slfq-pre-style'); }

// The value a style element's rule for the selector declares for a property, or null when it declares none.
function sheetStyle(sheet, selector, property) {
  var text = sheet.textContent;
  var start = text.indexOf(selector + '{');

  if (start < 0) return null;

  var found = null;

  text.slice(start + selector.length + 1, text.indexOf('}', start)).split(';').forEach(function (declaration) {
    var colon = declaration.indexOf(':');

    if (colon > 0 && declaration.slice(0, colon).trim() === property) found = declaration.slice(colon + 1).trim();
  });

  return found;
}

// Whether every rule in the style needs the given selector at its start, so it applies to nothing else.
function everyRuleStartsWith(sheet, selector) {
  return sheet.textContent.split('\n').every(function (rule) {
    return rule.split('{')[0].split(',').every(function (part) { return part.trim().indexOf(selector) === 0; });
  });
}
