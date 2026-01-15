export function getBeltConfig(belt = 'White') {
    const rank =
        belt.charAt(0).toUpperCase() + belt.slice(1).toLowerCase();

    const config = {
        White: { main: '#ffffff', text: '#333333', accent: '#667eea' },
        Grey: { main: '#9e9e9e', text: '#ffffff', accent: '#757575' },
        Yellow: { main: '#ffeb3b', text: '#333333', accent: '#fbc02d' },
        Orange: { main: '#ff9800', text: '#ffffff', accent: '#ff9800' },
        Green: { main: '#4caf50', text: '#ffffff', accent: '#4caf50' },
        Blue: { main: '#2196f3', text: '#ffffff', accent: '#2196f3' },
        Purple: { main: '#9c27b0', text: '#ffffff', accent: '#9c27b0' },
        Brown: { main: '#795548', text: '#ffffff', accent: '#795548' },
        Black: { main: '#212121', text: '#ffffff', accent: '#212121' },
    };

    return config[rank] || config.White;
}

export function formatDate(value?: string) {
    if (!value || value === 'Never') return 'Never';

    const date = new Date(value);
    return isNaN(date.getTime())
        ? value
        : date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
        });
}
