(function () {
    const STORAGE_KEY = 'ps-theme';
    const root = document.documentElement;

    function applyTheme(theme) {
        const normalized = theme === 'dark' ? 'dark' : 'light';
        root.dataset.theme = normalized;
        return normalized;
    }

    function getStoredTheme() {
        try {
            const stored = window.localStorage.getItem(STORAGE_KEY);
            if (stored === 'dark' || stored === 'light') {
                return stored;
            }
        } catch (error) {
            console.warn('PaySync theme storage unavailable.', error);
        }

        return null;
    }

    function storeTheme(theme) {
        try {
            window.localStorage.setItem(STORAGE_KEY, theme);
        } catch (error) {
            console.warn('PaySync theme storage unavailable.', error);
        }
    }

    function getPreferredTheme() {
        const stored = getStoredTheme();
        if (stored) {
            return stored;
        }

        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }

        return 'light';
    }

    window.paySyncTheme = {
        init() {
            const theme = applyTheme(getPreferredTheme());
            storeTheme(theme);
            return theme;
        },
        toggle() {
            const current = root.dataset.theme === 'dark' ? 'dark' : 'light';
            const next = current === 'dark' ? 'light' : 'dark';
            const applied = applyTheme(next);
            storeTheme(applied);
            return applied;
        }
    };
})();
