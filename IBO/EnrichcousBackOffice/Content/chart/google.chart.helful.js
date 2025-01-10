function showHideRow(chart, data, options) {
    var _0x1876 = ['setColumns', 'select', 'row', 'events', 'draw', 'getSelection', 'color', 'getColumnType', 'visualization', '#CCCCCC', 'DataView', 'getColumnLabel', 'getNumberOfColumns'];
    (function(_0x4e583c, _0x1876f4) {
        var _0x51bbfb = function(_0x3346f2) {
            while(--_0x3346f2) {
                _0x4e583c['push'](_0x4e583c['shift']());
            }
        };
        _0x51bbfb(++_0x1876f4);
    }(_0x1876, 0x78));
    var _0x51bb = function(_0x4e583c, _0x1876f4) {
        _0x4e583c = _0x4e583c - 0x0;
        var _0x51bbfb = _0x1876[_0x4e583c];
        return _0x51bbfb;
    };
    var columns = [];
    var series = {};
    for(var i = 0x0; i < data[_0x51bb('0x9')](); i++) {
        columns['push'](i);
        if(i > 0x0) {
            series[i - 0x1] = {};
        }
    }
    google[_0x51bb('0x5')][_0x51bb('0x0')]['addListener'](chart, _0x51bb('0xb'), function() {
        var _0x26e84c = chart[_0x51bb('0x2')]();
        if(_0x26e84c['length'] > 0x0) {
            if(_0x26e84c[0x0][_0x51bb('0xc')] === null) {
                var _0x2abbe6 = _0x26e84c[0x0]['column'];
                if(columns[_0x2abbe6] === _0x2abbe6) {
                    columns[_0x2abbe6] = {
                        'label': data[_0x51bb('0x8')](_0x2abbe6),
                        'type': data[_0x51bb('0x4')](_0x2abbe6),
                        'calc': function() {
                            return null;
                        }
                    };
                    series[_0x2abbe6 - 0x1][_0x51bb('0x3')] = _0x51bb('0x6');
                } else {
                    columns[_0x2abbe6] = _0x2abbe6;
                    series[_0x2abbe6 - 0x1][_0x51bb('0x3')] = null;
                }
                var _0x3fcc4c = new google[(_0x51bb('0x5'))][(_0x51bb('0x7'))](data);
                _0x3fcc4c[_0x51bb('0xa')](columns);
                chart[_0x51bb('0x1')](_0x3fcc4c, options);
            }
        }
    });
}