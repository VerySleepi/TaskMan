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

function toggleAddMemberPopup() {
    const overlay = document.getElementById('memberPopup-overlay');
    const content = document.getElementById('memberPopup-content');
    const groupId = document.getElementById('addMemberBtn').dataset.groupId;

    if (overlay.classList.contains('hidden')) {
        fetch(`/Groups/AddMembers?groupId=${groupId}`)
            .then(response => response.text())
            .then(html => {
                content.innerHTML = html;
                overlay.classList.remove('hidden');
                memberPanelSelection('addSelectedIds');
            })
            .catch(err => console.error("Error loading AddMember form:", err))
    }
    else {
        overlay.classList.add('hidden');
        content.innerHTML = '';
    }
}

function toggleRemoveMemberPopup() {
    const overlay = document.getElementById('memberPopup-overlay');
    const content = document.getElementById('memberPopup-content');
    const groupId = document.getElementById('removeMemberBtn').dataset.groupId;

    if (overlay.classList.contains('hidden')) {
        fetch(`/Groups/RemoveMembers?groupId=${groupId}`)
            .then(response => response.text())
            .then(html => {
                content.innerHTML = html;
                overlay.classList.remove('hidden');
                memberPanelSelection('removeSelectedIds');
            })
            .catch(err => console.error("Error loading RemoveMember form:", err))
    }
    else {
        overlay.classList.add('hidden');
        content.innerHTML = '';
    }
}

function memberPanelSelection(selectedIdsContainer) {
    const panels = document.querySelectorAll('.member-panel');
    const form = document.getElementById(selectedIdsContainer).closest('form');

    panels.forEach(panel => {
        panel.addEventListener('click', () => {
            panel.classList.toggle('selected');
        });
    });

    form.addEventListener('submit', (ev) => {
        ev.preventDefault();
        const selectedIds = document.getElementById(selectedIdsContainer);
        selectedIds.innerHTML = '';

        document.querySelectorAll('.member-panel.selected').forEach(panel => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'userIds';
            input.value = panel.dataset.userId;
            selectedIds.appendChild(input);
        });
        form.submit();
    });
}


document.addEventListener('DOMContentLoaded', function () {
    const createGroupBtn = document.getElementById('createGroupBtn');
    if (createGroupBtn) {
        createGroupBtn.addEventListener('click', (ev) => {
            ev.preventDefault();
            toggleCreateGroupPopup();
        });
    }

    const groupCloseBtn = document.getElementById('newGroup-close');
    if (groupCloseBtn) {
        groupCloseBtn.addEventListener('click', () => {
            console.log("Button clicked!");
            toggleCreateGroupPopup();
        });
    }

    const addMemberBtn = document.getElementById('addMemberBtn');
    if (addMemberBtn) {
        addMemberBtn.addEventListener('click', (ev) => {
            ev.preventDefault();
            toggleAddMemberPopup();
        });
    }

    const removeMemberBtn = document.getElementById('removeMemberBtn');
    if (removeMemberBtn) {
        removeMemberBtn.addEventListener('click', (ev) => {
            ev.preventDefault();
            toggleRemoveMemberPopup();
        });
    }

    const memberPopupClose = document.getElementById('memberPopup-close');
    if (memberPopupClose) {
        memberPopupClose.addEventListener('click', (ev) => {
            document.getElementById('memberPopup-overlay').classList.add('hidden');
            document.getElementById('memberPopup-content').innerHTML = '';
        });
    }
});