window.themeInterop = {
    applyTheme: function (theme) {
        try {
            if (!theme) theme = 'light';

            // If user wants 'system', detect current scheme and apply that class
            if (theme === 'system') {
                var isDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
                document.documentElement.classList.remove('dark-theme');
                document.documentElement.classList.remove('light-theme');
                document.documentElement.classList.add(isDark ? 'dark-theme' : 'light-theme');

                // ensure we have a listener to update when OS theme changes
                this._ensureSystemListener();
            } else {
                // explicit light/dark
                document.documentElement.classList.remove('dark-theme');
                document.documentElement.classList.remove('light-theme');
                document.documentElement.classList.add(theme === 'dark' ? 'dark-theme' : 'light-theme');

                // remove any system listener when forcing a theme
                this._removeSystemListener();
            }

            try { window.localStorage && window.localStorage.setItem && window.localStorage.setItem('theme', theme); } catch (e) { }
        } catch (e) {
            console && console.warn && console.warn('theme apply failed', e);
        }
    },
    getPreferredScheme: function () {
        try {
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) return 'dark';
            return 'light';
        } catch (e) {
            return 'light';
        }
    },

    // internal helpers to listen for system changes
    _systemListener: null,
    _ensureSystemListener: function () {
        if (this._systemListener) return;
        try {
            var mq = window.matchMedia('(prefers-color-scheme: dark)');
            var that = this;
            this._systemListener = function (e) {
                // only update if user preference is still 'system'
                try {
                    var stored = window.localStorage && window.localStorage.getItem && window.localStorage.getItem('theme');
                    if (stored !== 'system') return;
                } catch (ex) { }

                if (e.matches) {
                    document.documentElement.classList.remove('light-theme');
                    document.documentElement.classList.add('dark-theme');
                } else {
                    document.documentElement.classList.remove('dark-theme');
                    document.documentElement.classList.add('light-theme');
                }
            };
            if (mq.addEventListener) mq.addEventListener('change', this._systemListener);
            else if (mq.addListener) mq.addListener(this._systemListener);
        } catch (e) {
            // ignore
        }
    },
    _removeSystemListener: function () {
        if (!this._systemListener) return;
        try {
            var mq = window.matchMedia('(prefers-color-scheme: dark)');
            if (mq.removeEventListener) mq.removeEventListener('change', this._systemListener);
            else if (mq.removeListener) mq.removeListener(this._systemListener);
        } catch (e) { }
        this._systemListener = null;
    }
};
