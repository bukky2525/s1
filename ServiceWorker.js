const cacheName = "DefaultCompany-s1-1.0";
const contentToCache = [
    "Build/WebGL_Build.loader.js",
    "Build/WebGL_Build.framework.js",
    "Build/WebGL_Build.data",
    "Build/WebGL_Build.wasm",
    "TemplateData/style.css"
];

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');
    
    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
    })());
});

self.addEventListener('fetch', function (e) {
    // Chrome拡張機能やサポートされていないスキームのリクエストをフィルタリング
    if (e.request.url.startsWith('chrome-extension:') || 
        e.request.url.startsWith('moz-extension:') ||
        e.request.url.startsWith('webkit-extension:')) {
        return;
    }

    e.respondWith((async function () {
      try {
        let response = await caches.match(e.request);
        console.log(`[Service Worker] Fetching resource: ${e.request.url}`);
        if (response) { return response; }

        response = await fetch(e.request);
        const cache = await caches.open(cacheName);
        console.log(`[Service Worker] Caching new resource: ${e.request.url}`);
        cache.put(e.request, response.clone());
        return response;
      } catch (error) {
        console.log(`[Service Worker] Fetch failed: ${error}`);
        return fetch(e.request);
      }
    })());
});
