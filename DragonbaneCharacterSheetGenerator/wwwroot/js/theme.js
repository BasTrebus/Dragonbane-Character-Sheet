window.themeInterop = {
    applyTheme: function (theme) {
        try {
            if (!theme) theme = 'light';
            document.documentElement.classList.remove('dark-theme');
            document.documentElement.classList.remove('light-theme');
            document.documentElement.classList.add(theme === 'dark' ? 'dark-theme' : 'light-theme');
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
    }
};
