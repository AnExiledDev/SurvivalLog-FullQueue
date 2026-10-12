// Full Queue's web half. Runs in Root.html, the document every game page iframe sits in. The queue bar at the bottom
// of the HUD (CoreUI1) is drawn from a list the game's page cuts to the first five waiting orders. The plugin puts
// every waiting order into the game's update, and this script gives the bar the ones past the fifth too. With more
// than five the bar keeps the width of five slots and scrolls sideways with the mouse wheel, and the first and last
// whole slots in sight show how many orders are out of sight on their side. Slots scrolled out of sight stop taking
// clicks, so they cannot block clicks on the world behind them. With five or fewer the bar is left exactly as the game
// draws it. Sent again by the plugin every couple of seconds; the guard below makes a second copy return at once.
(function () {
  if (window.__slfullqueue) return;

  window.__slfullqueue = true;

  var POLL_MS = 200;
  var STYLE_ID = 'slfq-style';
  var GAME_SLOTS = 5;
  var UPDATE = 'WebUI_CoreUI1_ActionMsg';
  var BAR = '.queue-container';
  var LONG = 'slfq-long';
  var MOVED = 'slfq-moved';
  var BEFORE = 'data-slfq-before';
  var AFTER = 'data-slfq-after';
  var WHEEL_LINE_PX = 16;
  var EDGE_PX = 1;

  // The game's slots are 32.2px wide with 2.1px between them, so a long bar keeps the width of five. Every rule needs
  // the long class, which only a bar of more than five gets.
  var CSS = [
    BAR + '.' + LONG + '{box-sizing:content-box;max-width:calc(5 * 32.2px + 4 * 2.1px);overflow-x:auto;overflow-y:hidden;scrollbar-width:none}',
    BAR + '.' + LONG + '::-webkit-scrollbar{display:none}',
    BAR + '.' + LONG + ' > .queue-slot{flex-shrink:0}',
    BAR + '.' + LONG + ' > .queue-slot[' + BEFORE + ']::before,' + BAR + '.' + LONG + ' > .queue-slot[' + AFTER + ']::after{position:absolute;top:0;z-index:11;padding:0 2px;border-radius:2.8px;background:rgba(255,193,7,0.9);color:#222;font:bold 9px/12px Arial,sans-serif;pointer-events:none}',
    BAR + '.' + LONG + ' > .queue-slot[' + BEFORE + ']::before{content:attr(' + BEFORE + ');left:0}',
    BAR + '.' + LONG + ' > .queue-slot[' + AFTER + ']::after{content:attr(' + AFTER + ');right:0}'
  ].join('\n');

  var reported = {};

  // One note per kind of failure, so a page a game update reshaped says so once instead of every poll.
  function diag(kind, error) {
    if (reported[kind]) return;

    reported[kind] = true;

    try {
      console.warn('[Full Queue] ' + kind + ': ' + error);
    } catch (e) {}
  }

  function addStyle(doc) {
    if (doc.getElementById(STYLE_ID) || !doc.head) return;

    var style = doc.createElement('style');
    style.id = STYLE_ID;
    style.textContent = CSS;
    doc.head.appendChild(style);
  }

  // ===== The orders past the fifth =====

  // The game's own listener cuts the list to five. This one listens in the capture phase, which current browsers run
  // first, and takes the queue out of the update once it has put the whole list in, so the game's listener leaves the
  // list alone and the page draws it once. A browser that runs the game's listener first gets the whole list put back
  // right after it.
  function listen(page) {
    if (page.__slfq) return;

    page.__slfq = true;
    page.addEventListener('message', function (event) {
      try {
        takeUpdate(page, event.data);
      } catch (e) {
        diag('queue update', e);
      }
    }, true);
  }

  function takeUpdate(page, message) {
    if (!message || message.type !== UPDATE) return;

    var data = typeof message.data === 'string' && message.data ? page.JSON.parse(message.data) : message.data;

    if (!data || !data.queueJson) return;

    var orders = page.JSON.parse(data.queueJson);

    // The page draws the new list after this event, long or short, so the bar is fitted to it on the next frame.
    fitOnNextFrame(page);

    if (!Array.isArray(orders) || orders.length <= GAME_SLOTS) return;

    page.eval('state').queueList = orders.map(function (order) { return order || emptySlot(); });
    takeQueueOut(page, message, data);
  }

  function fitOnNextFrame(page) {
    page.requestAnimationFrame(function () {
      try {
        var bar = page.document.querySelector(BAR);

        if (bar) fitBar(bar);
      } catch (e) {
        diag('queue bar', e);
      }
    });
  }

  function takeQueueOut(page, message, data) {
    delete data.queueJson;

    if (typeof message.data === 'string') message.data = page.JSON.stringify(data);
  }

  // What the game's page puts in a slot with no order.
  function emptySlot() {
    return { icon: '', name: '', showIcon: false, isCancellable: false, isAiDriven: false, parentId: 0 };
  }

  // ===== The bar =====

  function fitBar(bar) {
    if (!bar.__slfq) {
      bar.__slfq = true;
      bar.addEventListener('wheel', scrollSideways, { passive: false });
      bar.addEventListener('scroll', onScrolled);
    }

    var slots = slotsIn(bar);
    var isLong = slots.length > GAME_SLOTS;

    if (bar.classList.contains(LONG) !== isLong) bar.classList.toggle(LONG, isLong);

    // The game's click-through only lets the world have a click outside elements marked like this, so on a long bar
    // the gaps between slots belong to the bar too and the wheel over them scrolls it.
    if (isLong && !bar.hasAttribute('data-interactive')) bar.setAttribute('data-interactive', '');
    if (!isLong && bar.hasAttribute('data-interactive')) bar.removeAttribute('data-interactive');

    fitSlots(bar, slots, isLong);
  }

  function slotsIn(bar) {
    var slots = [];

    for (var i = 0; i < bar.children.length; i++) {
      if (bar.children[i].classList.contains('queue-slot')) slots.push(bar.children[i]);
    }

    return slots;
  }

  // A slot scrolled fully out of sight still has a box on screen to the game's click-through, which would swallow
  // clicks on the world there. Turning its pointer events off takes it out of that list until it scrolls back. The
  // first and last whole slots in sight carry the count of orders out of sight on their side.
  function fitSlots(bar, slots, isLong) {
    var left = bar.getBoundingClientRect().left + bar.clientLeft;
    var right = left + bar.clientWidth;
    var hiddenBefore = 0;
    var hiddenAfter = 0;
    var firstWhole = -1;
    var lastWhole = -1;

    for (var i = 0; i < slots.length; i++) {
      var box = slots[i].getBoundingClientRect();
      var isInSight = !isLong || (box.right > left + EDGE_PX && box.left < right - EDGE_PX);
      var isWhole = !isLong || (box.left >= left - EDGE_PX && box.right <= right + EDGE_PX);

      setPointerEvents(slots[i], isInSight ? '' : 'none');

      if (isWhole) {
        if (firstWhole < 0) firstWhole = i;

        lastWhole = i;
      } else if (slots[i].classList.contains('filled')) {
        if (box.left < left) hiddenBefore++;
        else hiddenAfter++;
      }
    }

    for (var j = 0; j < slots.length; j++) {
      mark(slots[j], BEFORE, j === firstWhole ? countText(hiddenBefore) : null);
      mark(slots[j], AFTER, j === lastWhole ? countText(hiddenAfter) : null);
    }
  }

  function countText(hidden) { return hidden > 0 ? '+' + hidden : null; }

  function setPointerEvents(slot, wanted) {
    if (slot.style.pointerEvents !== wanted) slot.style.pointerEvents = wanted;
  }

  function mark(slot, name, text) {
    if (text === null) {
      if (slot.hasAttribute(name)) slot.removeAttribute(name);

      return;
    }

    if (slot.getAttribute(name) !== text) slot.setAttribute(name, text);
  }

  function scrollSideways(event) {
    var bar = event.currentTarget;
    var room = bar.scrollWidth - bar.clientWidth;

    if (!bar.classList.contains(LONG) || room <= 0) return;

    var delta = Math.abs(event.deltaX) > Math.abs(event.deltaY) ? event.deltaX : event.deltaY;

    if (event.deltaMode === 1) delta *= WHEEL_LINE_PX;
    if (event.deltaMode === 2) delta *= bar.clientWidth;

    event.preventDefault();
    bar.scrollLeft = Math.max(0, Math.min(room, bar.scrollLeft + delta));
  }

  function onScrolled(event) {
    var bar = event.currentTarget;

    if (bar.__slfqFrame) return;

    var view = bar.ownerDocument.defaultView;

    // The page can be dropped between the scroll and this frame.
    if (!view) return;

    bar.__slfqFrame = true;
    view.requestAnimationFrame(function () {
      bar.__slfqFrame = false;

      try {
        fitBar(bar);

        // The game re-reads its clickable areas when a class changes, not when an element scrolls.
        bar.classList.toggle(MOVED);
      } catch (e) {
        diag('queue bar', e);
      }
    });
  }

  // ===== Finding the page =====

  // The game's pages are same-origin iframes of this document, created and dropped whenever the game likes.
  function patchPages() {
    var frames = document.querySelectorAll('iframe');

    for (var i = 0; i < frames.length; i++) {
      var src = frames[i].src || '';

      if (/CoreUI1\.html/.test(src)) patchFrame(frames[i], src);
    }
  }

  function patchFrame(frame, src) {
    var page = null;
    var doc = null;

    try {
      page = frame.contentWindow;
      doc = frame.contentDocument;
    } catch (e) {}

    if (!page || !doc) return diag('page access', 'cannot reach ' + src);
    if (doc.readyState === 'loading') return;

    try {
      listen(page);
      addStyle(doc);

      var bar = doc.querySelector(BAR);

      if (bar) fitBar(bar);
    } catch (e) {
      diag('CoreUI1', e);
    }
  }

  setInterval(function () {
    try {
      patchPages();
    } catch (e) {
      diag('poll', e);
    }
  }, POLL_MS);
})();
