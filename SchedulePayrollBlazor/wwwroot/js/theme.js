(function () {
    const STORAGE_KEY = 'ps-theme';
    const root = document.documentElement;

    function normalizeTheme(theme) {
        return theme === 'dark' ? 'dark' : 'light';
    }

    function applyTheme(theme) {
        const normalized = normalizeTheme(theme);
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
            window.localStorage.setItem(STORAGE_KEY, normalizeTheme(theme));
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
        setTheme(theme) {
            const applied = applyTheme(theme);
            storeTheme(applied);
            return applied;
        },
        getTheme() {
            return root.dataset.theme === 'dark' ? 'dark' : 'light';
        }
    };
})();
