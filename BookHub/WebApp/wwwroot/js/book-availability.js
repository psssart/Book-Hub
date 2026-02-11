(function () {
    'use strict';

    var map, markers = [], selectedMarker = null;
    var defaultIcon, selectedIcon, outOfStockIcon;
    var warehouseData = [];
    var bookWarehouseData = {};

    function init(allWarehouses, bookWarehouses) {
        warehouseData = allWarehouses;
        bookWarehouses.forEach(function (bw) {
            bookWarehouseData[bw.warehouseId] = bw;
        });

        createIcons();
        initMap();
        placeMarkers();
        bindFilters();
        tryGeolocation();
    }

    function createIcons() {
        defaultIcon = L.divIcon({
            className: 'warehouse-marker',
            html: '<div style="width:28px;height:28px;border-radius:50%;background:#6474C9;border:3px solid #fff;box-shadow:0 2px 6px rgba(0,0,0,0.3);"></div>',
            iconSize: [28, 28],
            iconAnchor: [14, 14],
            popupAnchor: [0, -16]
        });

        selectedIcon = L.divIcon({
            className: 'warehouse-marker selected',
            html: '<div style="width:36px;height:36px;border-radius:50%;background:#C9B964;border:3px solid #fff;box-shadow:0 2px 8px rgba(0,0,0,0.4);"></div>',
            iconSize: [36, 36],
            iconAnchor: [18, 18],
            popupAnchor: [0, -20]
        });

        outOfStockIcon = L.divIcon({
            className: 'warehouse-marker',
            html: '<div style="width:28px;height:28px;border-radius:50%;background:#adb5bd;border:3px solid #fff;box-shadow:0 2px 6px rgba(0,0,0,0.3);"></div>',
            iconSize: [28, 28],
            iconAnchor: [14, 14],
            popupAnchor: [0, -16]
        });
    }

    function initMap() {
        map = L.map('availability-map', {
            zoomControl: true,
            scrollWheelZoom: true
        }).setView([20, 0], 2);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 18
        }).addTo(map);
    }

    function placeMarkers() {
        warehouseData.forEach(function (w) {
            var bw = bookWarehouseData[w.id];
            var hasStock = bw && bw.count > 0;
            var icon = hasStock ? defaultIcon : outOfStockIcon;

            var directionsUrl = 'https://www.google.com/maps/dir/?api=1&destination=' + w.gpsX + ',' + w.gpsY;

            var popupContent = '<div class="warehouse-popup">' +
                '<h6>' + escapeHtml(w.name) + '</h6>' +
                '<p>' + escapeHtml(w.country) + '</p>' +
                (bw
                    ? '<p><strong>' + bw.count + '</strong> in stock</p>'
                    : '<p style="color:#999">Not available</p>') +
                '<a href="' + directionsUrl + '" target="_blank" rel="noopener" class="directions-btn">' +
                '<i class="bi bi-sign-turn-right-fill"></i> Get Directions</a>' +
                '</div>';

            var marker = L.marker([w.gpsX, w.gpsY], { icon: icon })
                .addTo(map)
                .bindPopup(popupContent);

            marker._warehouseId = w.id;
            marker._country = w.country;
            marker._visible = true;

            marker.on('click', function () {
                selectWarehouse(w.id);
            });

            markers.push(marker);
        });
    }

    function selectWarehouse(warehouseId) {
        // Reset previous selection
        if (selectedMarker) {
            var prevBw = bookWarehouseData[selectedMarker._warehouseId];
            var prevHasStock = prevBw && prevBw.count > 0;
            selectedMarker.setIcon(prevHasStock ? defaultIcon : outOfStockIcon);
        }

        // Find and highlight new selection
        var marker = markers.find(function (m) { return m._warehouseId === warehouseId; });
        if (!marker) return;

        selectedMarker = marker;
        marker.setIcon(selectedIcon);
        marker.openPopup();

        map.flyTo(marker.getLatLng(), 6, { duration: 1 });

        // Update warehouse selector
        var selector = document.getElementById('warehouseSelector');
        if (selector) selector.value = warehouseId;

        // Update info panel
        updateInfoPanel(warehouseId);
    }

    function updateInfoPanel(warehouseId) {
        var panel = document.getElementById('infoPanel');
        if (!panel) return;

        var w = warehouseData.find(function (w) { return w.id === warehouseId; });
        if (!w) {
            panel.innerHTML = '<p class="text-center" style="color: var(--text-muted);">Select a warehouse to see details</p>';
            panel.classList.add('empty');
            return;
        }

        var bw = bookWarehouseData[warehouseId];
        panel.classList.remove('empty');

        var stockHtml;
        if (bw && bw.count > 0) {
            stockHtml = '<span class="stock-badge in-stock"><i class="bi bi-check-circle-fill"></i> ' + bw.count + ' in stock</span>';
        } else {
            stockHtml = '<span class="stock-badge out-of-stock"><i class="bi bi-x-circle-fill"></i> Not available</span>';
        }

        var lastSupply = 'N/A';
        if (bw && bw.lastSupply) {
            var date = new Date(bw.lastSupply);
            lastSupply = date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
        }

        panel.innerHTML =
            '<h5 class="info-panel-header">' + escapeHtml(w.name) + '</h5>' +
            '<div class="info-panel-grid">' +
            '  <div class="info-panel-item">' +
            '    <span class="info-panel-label">Country</span>' +
            '    <span class="info-panel-value">' + escapeHtml(w.country) + '</span>' +
            '  </div>' +
            '  <div class="info-panel-item">' +
            '    <span class="info-panel-label">Stock</span>' +
            '    <span class="info-panel-value">' + stockHtml + '</span>' +
            '  </div>' +
            '  <div class="info-panel-item">' +
            '    <span class="info-panel-label">Last Supply</span>' +
            '    <span class="info-panel-value">' + lastSupply + '</span>' +
            '  </div>' +
            '  <div class="info-panel-item">' +
            '    <span class="info-panel-label">Coordinates</span>' +
            '    <span class="info-panel-value">' + w.gpsX.toFixed(4) + ', ' + w.gpsY.toFixed(4) + '</span>' +
            '  </div>' +
            '</div>';
    }

    function bindFilters() {
        var countrySelect = document.getElementById('countryFilter');
        var warehouseSelect = document.getElementById('warehouseSelector');

        if (countrySelect) {
            countrySelect.addEventListener('change', function () {
                var country = this.value;
                filterByCountry(country);
                updateWarehouseSelector(country);
            });
        }

        if (warehouseSelect) {
            warehouseSelect.addEventListener('change', function () {
                var id = this.value;
                if (id) selectWarehouse(id);
            });
        }
    }

    function filterByCountry(country) {
        markers.forEach(function (m) {
            if (!country || m._country === country) {
                if (!m._visible) {
                    m.addTo(map);
                    m._visible = true;
                }
            } else {
                if (m._visible) {
                    map.removeLayer(m);
                    m._visible = false;
                }
            }
        });

        // Fit bounds to visible markers
        var visible = markers.filter(function (m) { return m._visible; });
        if (visible.length > 0) {
            var group = L.featureGroup(visible);
            map.fitBounds(group.getBounds().pad(0.2));
        }
    }

    function updateWarehouseSelector(country) {
        var select = document.getElementById('warehouseSelector');
        if (!select) return;

        var current = select.value;
        select.innerHTML = '<option value="">-- All warehouses --</option>';

        warehouseData
            .filter(function (w) { return !country || w.country === country; })
            .forEach(function (w) {
                var bw = bookWarehouseData[w.id];
                var stockText = bw ? ' (' + bw.count + ' in stock)' : ' (N/A)';
                var opt = document.createElement('option');
                opt.value = w.id;
                opt.textContent = w.name + stockText;
                select.appendChild(opt);
            });

        // Restore selection if still visible
        if (current) {
            var opt = select.querySelector('option[value="' + current + '"]');
            if (opt) select.value = current;
        }
    }

    function tryGeolocation() {
        if (!navigator.geolocation) return;

        navigator.geolocation.getCurrentPosition(function (pos) {
            var userLat = pos.coords.latitude;
            var userLng = pos.coords.longitude;
            var nearest = null;
            var minDist = Infinity;

            // Only consider warehouses that have this book in stock
            warehouseData.forEach(function (w) {
                var bw = bookWarehouseData[w.id];
                if (!bw || bw.count <= 0) return;
                var d = haversine(userLat, userLng, w.gpsX, w.gpsY);
                if (d < minDist) {
                    minDist = d;
                    nearest = w;
                }
            });

            if (nearest) {
                selectWarehouse(nearest.id);
            }
        }, function () {
            // Geolocation denied or unavailable — no action needed
        }, { timeout: 5000 });
    }

    function haversine(lat1, lon1, lat2, lon2) {
        var R = 6371;
        var dLat = toRad(lat2 - lat1);
        var dLon = toRad(lon2 - lon1);
        var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
            Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) *
            Math.sin(dLon / 2) * Math.sin(dLon / 2);
        return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    }

    function toRad(deg) {
        return deg * Math.PI / 180;
    }

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(str));
        return div.innerHTML;
    }

    // Expose init function
    window.BookAvailability = { init: init };
})();
