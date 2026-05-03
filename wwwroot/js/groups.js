function toggleCreateGroupPopup() {
    const overlay = document.getElementById('newGroup-overlay');
    const content = document.getElementById('newGroup-content');

    console.log(overlay)

    if (overlay.classList.contains('hidden')) {
        fetch("/Groups/Create")
            .then(response => response.text())
            .then(html => {
                content.innerHTML = html;
                overlay.classList.remove('hidden');
            })
            .catch(err => console.error("Error loading CreateGroup form:", err))
    } else {
        overlay.classList.add('hidden');
        content.innerHTML = '';
    }
}


document.addEventListener('DOMContentLoaded', function () {
    const createGroupBtn = document.getElementById('createGroupBtn');
    if (createGroupBtn) {
        createGroupBtn.addEventListener('click', (ev) => {
            ev.preventDefault();
            toggleCreateGroupPopup();
        });
    }
});