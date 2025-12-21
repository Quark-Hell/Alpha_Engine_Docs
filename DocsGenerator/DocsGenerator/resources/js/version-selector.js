document.addEventListener('DOMContentLoaded', function () {
    const versionSelect = document.getElementById('version-select');
    if (!versionSelect) return;

    const currentPath = window.location.pathname;
    const versionMatch = currentPath.match(/\/(v[\d.]+)\//);
    if (versionMatch) {
        versionSelect.value = versionMatch[1];
    }

    versionSelect.addEventListener('change', function () {
        const selectedVersion = this.value;
        let newUrl = selectedVersion === 'latest'
            ? '/docs/latest/html/index.html'
            : `/docs/${selectedVersion}/html/index.html`;
        window.location.href = newUrl;
    });
});