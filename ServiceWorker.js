const cacheName = "DefaultCompany-s1-1.1";
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
    // Chrome拡張やサポートされていないスキームのリクエストをフィルタリング
    if (e.request.url.startsWith('chrome-extension:') ||
        e.request.url.startsWith('moz-extension:') ||
        e.request.url.startsWith('webkit-extension:')) {
        return;
    }

    // POSTリクエストやその他のキャッシュできないリクエストを早期にスキップ
    if (e.request.method !== 'GET') {
        console.log(`[Service Worker] Skipping non-GET request: ${e.request.method} ${e.request.url}`);
        e.respondWith(fetch(e.request));
        return;
    }

    // blobやdataスキームのリクエストもスキップ
    if (e.request.url.startsWith('blob:') || e.request.url.startsWith('data:')) {
        console.log(`[Service Worker] Skipping blob/data request: ${e.request.url}`);
        e.respondWith(fetch(e.request));
        return;
    }

    e.respondWith((async function () {
      try {
        let response = await caches.match(e.request);
        console.log(`[Service Worker] Fetching resource: ${e.request.url}`);
        if (response) { 
            console.log(`[Service Worker] Cache hit: ${e.request.url}`);
            return response; 
        }

        response = await fetch(e.request);
        
        // レスポンスが正常で、GETリクエストで、キャッシュ可能な場合のみキャッシュ
        if (response && response.status === 200 && e.request.method === 'GET' && response.type === 'basic') {
            try {
                const cache = await caches.open(cacheName);
                console.log(`[Service Worker] Caching new resource: ${e.request.url}`);
                // クローンを作成してからキャッシュ
                await cache.put(e.request, response.clone());
            } catch (cacheError) {
                console.log(`[Service Worker] Failed to cache: ${e.request.url}`, cacheError);
            }
        }
        
        return response;
      } catch (error) {
        console.log(`[Service Worker] Fetch failed: ${error}`);
        // フォールバック：ネットワークから直接取得を試行
        try {
            return await fetch(e.request);
        } catch (fallbackError) {
            console.log(`[Service Worker] Fallback fetch failed: ${fallbackError}`);
            throw fallbackError;
        }
      }
    })());
});
