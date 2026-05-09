let selectedGroupTaskId = null;
let panelOpen = false;

function toggleCreateGroupPopup() {
    const overlay = document.getElementById('newGroup-overlay');
    const content = document.getElementById('newGroup-content');

    console.log(overlay)

    if (overlay.classList.contains('hidden')) {
        fetch("/Groups/CreateGroup")
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

function toggleEditGroupPopup() {
    let editGroupBtns = document.querySelectorAll('.groupEditBtn');
    const overlay = document.getElementById('editGroup-overlay');
    const content = document.getElementById('editGroup-content');

    editGroupBtns.forEach(btn => {
        btn.addEventListener('click', (ev) => {
            ev.preventDefault();
            const groupId = btn.dataset.groupId;
            fetch(`/Groups/EditGroup?groupId=${groupId}`)
                .then(response => response.text())
                .then(html => {
                    content.innerHTML = html;
                    overlay.classList.remove('hidden');
                })
                .catch(err => console.error("Error loading EditGroup form:", err));

        });
    });

    const editGroupCloseBtn = document.getElementById('editGroup-close');
    if (editGroupCloseBtn) {
        editGroupCloseBtn.addEventListener('click', () => {
            overlay.classList.add('hidden');
            content.innerHTML = '';
        });
    }
}

function deleteGroup() {
    let deleteForm = null
    const deleteBtns = document.querySelectorAll('.deleteGroupBtn');

    deleteBtns.forEach(btn => {
        btn.addEventListener('click', (ev) => {
            ev.preventDefault();
            deleteForm = btn.closest('form');
            document.getElementById('deleteGroup-overlay').classList.remove('hidden');
        });
    });

    const confirmDeleteBtn = document.getElementById('deleteGroupConfirmBtn');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', () => {
            deleteForm.submit();
        });
    }

    const deleteGroupCancelBtn = document.getElementById('deleteGroupCancelBtn');
    if (deleteGroupCancelBtn) {
        deleteGroupCancelBtn.addEventListener('click', () => {
            document.getElementById('deleteGroup-overlay').classList.add('hidden');
        });
    }

}

function toggleCreateGroupTaskPopup() {
    const overlay = document.getElementById('groupTaskPopup-overlay');
    const content = document.getElementById('groupTaskPopup-content');
    const taskBtn = document.getElementById('newGroupTaskBtn');
    const groupId = taskBtn.dataset.groupId;
    const hasLists = taskBtn.dataset.hasLists === "true";

    if (!hasLists) {
        alert("You need to create a task list before adding any tasks!");
        return;
    }

    console.log(overlay)

    if (overlay.classList.contains('hidden')) {
        fetch(`/Groups/CreateGroupTask?groupId=${groupId}`)
            .then(response => response.text())
            .then(html => {
                content.innerHTML = html;
                overlay.classList.remove('hidden');
                groupTaskListDropdown();
            });
    }
    else {
        overlay.classList.add('hidden');
        content.innerHTML = '';
    }
    

}

function groupTaskListDropdown() {
    const dropdown = document.getElementById('groupTaskListDropdown');
    const toggle = document.getElementById('groupTaskListToggle');
    const dropdownList = dropdown.querySelector('.list-dropdown-list');
    const listInput = document.getElementById('groupTaskListID');
    const selectedGroupList = document.getElementById('groupTaskListSelected');
    const dropdownItems = dropdown.querySelectorAll('.list-dropdown-item button');

    toggle.addEventListener('click', (ev) => {
        ev.preventDefault();
        dropdownList.classList.toggle('hidden');
    });

    dropdownItems.forEach(button => {
        button.addEventListener('click', (ev) => {
            ev.preventDefault();
            listInput.value = button.value;
            selectedGroupList.textContent = button.textContent;
            dropdownList.classList.add('hidden');
        });
    });

    document.addEventListener('click', (ev) => {
        if (!dropdown.contains(ev.target)) {
            dropdownList.classList.add('hidden');
        }
    });
}

function toggleCreateGroupListPopup() {
    const overlay = document.getElementById('groupListPopup-overlay');
    const content = document.getElementById('groupListPopup-content');
    const groupId = document.getElementById('newGroupListBtn').dataset.groupId;

    if (overlay.classList.contains('hidden')) {
        fetch(`/Groups/CreateGroupList?groupId=${groupId}`)
            .then(response => response.text())
            .then(html => {
                content.innerHTML = html;
                overlay.classList.remove('hidden');
            })
            .catch(err => console.error("Error loading CreateGroupList form:", err))
    }
    else {
        overlay.classList.add('hidden');
        content.innerHTML = '';
    }
}

function openGroupPanel() {
    const sidePanel = document.getElementById('groupTaskDetailsPanel');
    const content = document.querySelector('.content');

    if (window.innerWidth <= 805) {
        sidePanel.classList.add('panel-open');
        content.classList.remove('panel-visible');
    }
    else {
        content.classList.add('panel-visible');
        sidePanel.classList.remove('panel-open');
        sidePanel.classList.remove('hidden');
    }

    panelOpen = true;
}

function closeGroupPanel() {
    const sidePanel = document.getElementById('groupTaskDetailsPanel');
    const content = document.querySelector('.content');

    if (window.innerWidth <= 805) {
        sidePanel.classList.remove('panel-open');
    }
    else {
        sidePanel.classList.add('hidden');
        content.classList.remove('panel-visible');
    }

    panelOpen = false;
}

function toggleGroupTaskPanel() {
    document.querySelectorAll('.taskObject').forEach(task => {
        task.addEventListener('click', (ev) => {
            const taskId = task.dataset.taskId;
            const title = task.dataset.taskTitle;
            const description = task.dataset.taskDescription;
            const dueDate = task.dataset.taskDueDate;
            const status = task.dataset.taskStatus;

            document.getElementById('panelGroupTitle').textContent = title;
            document.getElementById('panelGroupDescription').textContent = description;
            document.getElementById('panelGroupDueDate').textContent = dueDate;
            document.getElementById('panelGroupStatus').textContent = status === '1' ? 'Completed' : 'Not Completed';
            document.getElementById('groupTaskPanelCompleteInput').value = taskId;
            document.querySelector('.groupTaskPanelEditBtn').dataset.taskId = taskId;
            selectedGroupTaskId = taskId;

            openGroupPanel();
        });
    });
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

function editGroupTask() {
    const taskEditButtons = document.querySelectorAll('.groupTaskEditBtn, .groupTaskPanelEditBtn');
    taskEditButtons.forEach(btn => {
        btn.addEventListener('click', (ev) => {
            ev.stopPropagation();
            const taskId = btn.dataset.taskId;
            const overlay = document.getElementById('groupTaskPopup-overlay');
            const content = document.getElementById('groupTaskPopup-content');
            // Fetch pre-filled edit form from EditGroupTask GET action
            fetch(`/Groups/EditGroupTask?taskId=${taskId}`)
                .then(response => response.text())
                .then(html => {
                    console.log(content);
                    content.innerHTML = html;
                    overlay.classList.remove('hidden');
                    groupTaskListDropdown();
                })
                .catch(err => console.error("Error loading EditGroupTask form:", err));


        });
    });
}

function deleteGroupTask() {
    let deleteForm = null
    var taskDeleteBtn = document.querySelectorAll('.groupTaskDeleteBtn');

    taskDeleteBtn.forEach(btn => {
        btn.addEventListener('click', (ev) => {
            ev.stopPropagation();
            deleteForm = btn.closest('form');
            console.log("Delete btn clicked");
            document.getElementById('deleteGroupTaskOverlay').classList.remove('hidden');
        });
    });

    const deleteTaskConfirmBtn = document.getElementById('deleteGroupTaskConfirmBtn');
    if (deleteTaskConfirmBtn) {
        deleteTaskConfirmBtn.addEventListener('click', () => {
            deleteForm.submit();
        })
    }

    const deleteTaskCancelBtn = document.getElementById('deleteGroupTaskCancelBtn');
    if (deleteTaskCancelBtn) {
        deleteTaskCancelBtn.addEventListener('click', () => {
            document.getElementById('deleteGroupTaskOverlay').classList.add('hidden');
        })
    }
}

function deleteGroupList() {
    let deleteForm = null
    var listDeleteBtn = document.querySelectorAll('.listDeleteBtn');

    listDeleteBtn.forEach(btn => {
        btn.addEventListener('click', () => {
            deleteForm = btn.closest('form');
            console.log("Delete list btn clicked");
            document.getElementById('deleteGroupListOverlay').classList.remove('hidden');
        });
    });

    const deleteListConfirmBtn = document.getElementById('deleteGroupListConfirmBtn');
    if (deleteListConfirmBtn) {
        deleteListConfirmBtn.addEventListener('click', () => {
            deleteForm.submit();
        })
    }

    const deleteListCancelBtn = document.getElementById('deleteGroupListCancelBtn');
    if (deleteListCancelBtn) {
        deleteListCancelBtn.addEventListener('click', () => {
            document.getElementById('deleteGroupListOverlay').classList.add('hidden');
        });
    }
}

document.addEventListener('DOMContentLoaded', function () {

    if (window.innerWidth <= 805) {
        document.getElementById('groupTaskDetailsPanel').classList.remove('hidden');
    }

    window.addEventListener('resize', () => {
        const sidePanel = document.getElementById('groupTaskDetailsPanel');

        if (panelOpen) {
            closeGroupPanel();
            openGroupPanel();
        }
        else {
            if (window.innerWidth >= 805) {
                sidePanel.classList.add('hidden');
            }
            else {
                sidePanel.classList.remove('hidden');
            }
        }
    });

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
            console.log("Button clicked!")
            toggleCreateGroupPopup();
        });
    }

    const createGroupTaskBtn = document.getElementById('newGroupTaskBtn');
    if (createGroupTaskBtn) {
        createGroupTaskBtn.addEventListener('click', (ev) => {
            ev.preventDefault();
            toggleCreateGroupTaskPopup();
        });
    }

    const groupTaskCloseBtn = document.getElementById('groupTaskPopup-close');
    if (groupTaskCloseBtn) {
        groupTaskCloseBtn.addEventListener('click', (ev) => {
            ev.preventDefault();
            toggleCreateGroupTaskPopup();
        });
    }

    const createGroupListBtn = document.getElementById('newGroupListBtn');
    if (createGroupListBtn) {
        createGroupListBtn.addEventListener('click', (ev) => {
            ev.preventDefault();
            toggleCreateGroupListPopup();
        });
    }

    const closeGroupListBtn = document.getElementById('groupListPopup-close');
    if (closeGroupListBtn) {
        closeGroupListBtn.addEventListener('click', () => {
            console.log("Button clicked!");
            toggleCreateGroupListPopup();
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

    var taskStatusCheckboxes = document.querySelectorAll('.task-checkbox');
    taskStatusCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', (ev) => {
            ev.stopPropagation();
            checkbox.closest('form').submit();           
        });
        if (checkbox.checked) {
            checkbox.closest('.taskObject').classList.add('completed');
        }

    });

    const panelCloseBtn = document.querySelector('.panelCloseBtn');
    if (panelCloseBtn) {
        panelCloseBtn.addEventListener('click', () => {
            closeGroupPanel();
        });
    }

    toggleGroupTaskPanel();
    toggleEditGroupPopup();
    deleteGroup();
    editGroupTask();
    deleteGroupList();
    deleteGroupTask();
});