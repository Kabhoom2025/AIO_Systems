/**
 * AI Chatbot embeddable widget loader.
 *
 * Usage (paste once, anywhere on the page — typically just before </body>):
 *
 *   <script
 *     src="https://YOUR-CHATBOT-DOMAIN/embed/widget-loader.js"
 *     data-app-url="https://YOUR-CHATBOT-DOMAIN"
 *     data-position="bottom-right"
 *   ></script>
 *
 * This script itself is tiny and dependency-free — it never touches your site's CSS/JS. It
 * only draws a floating button and, when clicked, an <iframe> pointing at the chatbot's own
 * /widget page (which is the full Angular app, served from the chatbot's own domain — so
 * everything inside the iframe is same-origin to the chatbot's API, no CORS setup needed).
 */
(function () {
  var currentScript = document.currentScript;
  var config = (currentScript && currentScript.dataset) || {};
  var appUrl = (config.appUrl || '').replace(/\/$/, '');
  if (!appUrl) {
    console.error('[ai-chat-widget] Missing required data-app-url attribute on the loader <script> tag.');
    return;
  }

  var position = config.position === 'bottom-left' ? 'bottom-left' : 'bottom-right';
  var accentColor = config.accentColor || '#673ab7';
  var sideStyle = position === 'bottom-left' ? 'left: 20px;' : 'right: 20px;';

  var style = document.createElement('style');
  style.textContent =
    '.ai-chat-widget-bubble {' +
    '  position: fixed; bottom: 20px; ' + sideStyle +
    '  width: 60px; height: 60px; border-radius: 50%;' +
    '  background: ' + accentColor + '; color: #fff; border: none; cursor: pointer;' +
    '  box-shadow: 0 4px 16px rgba(0,0,0,0.25); z-index: 2147483000;' +
    '  display: flex; align-items: center; justify-content: center;' +
    '  transition: transform 0.15s ease;' +
    '}' +
    '.ai-chat-widget-bubble:hover { transform: scale(1.06); }' +
    '.ai-chat-widget-bubble svg { width: 28px; height: 28px; fill: #fff; }' +
    '.ai-chat-widget-panel {' +
    '  position: fixed; bottom: 92px; ' + sideStyle +
    '  width: 380px; max-width: calc(100vw - 40px);' +
    '  height: 600px; max-height: calc(100vh - 120px);' +
    '  border-radius: 16px; overflow: hidden; box-shadow: 0 12px 40px rgba(0,0,0,0.3);' +
    '  z-index: 2147483000; display: none; background: #fff;' +
    '}' +
    '.ai-chat-widget-panel.open { display: block; }' +
    '.ai-chat-widget-panel iframe { width: 100%; height: 100%; border: none; }' +
    '@media (max-width: 480px) {' +
    '  .ai-chat-widget-panel { width: 100vw; height: 100vh; max-height: 100vh; bottom: 0; right: 0; left: 0; border-radius: 0; }' +
    '}';
  document.head.appendChild(style);

  var bubble = document.createElement('button');
  bubble.className = 'ai-chat-widget-bubble';
  bubble.type = 'button';
  bubble.setAttribute('aria-label', 'Open AI Assistant chat');
  bubble.innerHTML =
    '<svg viewBox="0 0 24 24"><path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2z"/></svg>';

  var panel = document.createElement('div');
  panel.className = 'ai-chat-widget-panel';

  var iframeCreated = false;

  function ensureIframe() {
    if (iframeCreated) return;
    var iframe = document.createElement('iframe');
    iframe.src = appUrl + '/widget';
    iframe.title = 'AI Assistant chat';
    iframe.setAttribute('allow', 'microphone; autoplay');
    panel.appendChild(iframe);
    iframeCreated = true;
  }

  function openWidget() {
    ensureIframe();
    panel.classList.add('open');
  }

  function closeWidget() {
    panel.classList.remove('open');
  }

  bubble.addEventListener('click', function () {
    if (panel.classList.contains('open')) {
      closeWidget();
    } else {
      openWidget();
    }
  });

  window.addEventListener('message', function (event) {
    if (event && event.data && event.data.type === 'ai-chat-widget:close') {
      closeWidget();
    }
  });

  document.body.appendChild(panel);
  document.body.appendChild(bubble);
})();
