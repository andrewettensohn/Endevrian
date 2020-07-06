function displayDeleteSectionName(sessionSectionId) {

    let sectionName = $("#manageSectionName" + sessionSectionId)
    let input = $("#inputManageSectionName" + sessionSectionId)
    let message = $("#manageSectionMessage");
    let btnDelete = $("#btnDeleteSectionName" + sessionSectionId);
    let btnEdit = $("#btnEditSectionName" + sessionSectionId);

    message.text("Enter the name of the section and click delete to confirm");

    sectionName.toggleClass("d-none");
    input.toggleClass("d-none")
    message.toggleClass("d-none");
    $("#btnManageSectionDelete").toggleClass("d-none");

    if (btnEdit.prop("disabled")) {

        $("#tbodyManageSections").find("button").prop("disabled", false);

    } else {

        $("#tbodyManageSections").find("button").prop("disabled", true);
        btnDelete.prop("disabled", false)

    }

    input.attr("placeholder", sectionName.text())
    input.focus()

    $("#btnManageSectionDelete").on("click", async function (evt) {

        let confirmSectionName = $("#inputManageSectionName" + sessionSectionId).val();

        if (confirmSectionName == sectionName.text()) {

            fetch("api/SessionSection/" + sessionSectionId, {
                method: 'DELETE',
            })
                .then(() => location.reload())
                .catch(() => console.log("Failed to delete session section"));


        } else {

            message.text("Name does not match.");
            message.addClass("text-danger");

        }

    });
}

function displayEditSectionName(sessionSectionId) {

    let currentName = $("#manageSectionName" + sessionSectionId);
    let input = $("#inputManageSectionName" + sessionSectionId);
    let btnDelete = $("#btnDeleteSectionName" + sessionSectionId);
    let btnEdit = $("#btnEditSectionName" + sessionSectionId);

    currentName.toggleClass("d-none");
    input.toggleClass("d-none");
    $("#btnManageSectionUpdate").toggleClass("d-none");

    if (btnDelete.prop("disabled")) {

        $("#tbodyManageSections").find("button").prop("disabled", false);

    } else {

        $("#tbodyManageSections").find("button").prop("disabled", true);
        btnEdit.prop("disabled", false)

    }

    input.val(currentName.text())
    input.focus();

    $("#btnManageSectionUpdate").on("click", async function (evt) {

        let updatedSessionSectionName = $("#inputManageSectionName" + sessionSectionId).val();

        item = {

            sessionSectionID: parseInt(sessionSectionId, 10),
            sessionSectionName: updatedSessionSectionName

        }

        fetch("api/SessionSection/" + sessionSectionId, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(item)
        })
            .then(() => location.reload())
            .catch(() => console.log("Failed to update session section name"));

    });
}

function deleteSelectedNote(selectedNoteId) {

    fetch("api/SessionNote/" + selectedNoteId, {
        method: 'DELETE'
    })
        .then(() => location.reload())
        .catch(() => console.log("Failed to delete item"));

}

function displayEditNote() {

    $("#sectionNoteBody").toggleClass("d-none");
    $("#sectionEditNote").toggleClass("d-none");
    let currentNoteBodyText = $("#noteBody").html();
    $("#textAreaLogBody").html(currentNoteBodyText);
    attachEditorSessionNote();

}

function attachEditorSessionNote() {

    let editor;

    ClassicEditor
        .create(document.querySelector('#textAreaLogBody'))
        .then(newEditor => {
            editor = newEditor;
        })
        .catch(error => {
            console.error(error);
        });

    let nameInput = $("#inputUpdateSessionNoteName");
    let noteName = $("#sessionNoteName");

    nameInput.toggleClass("d-none")
    noteName.toggleClass("d-none");
    nameInput.val(noteName.text());

    let logForm = $("#sessionNoteForm");

    logForm.on("submit", async function (evt) {
        let editorData = editor.getData();

        evt.preventDefault();

        item = {

            sessionNoteTitle: $("#inputUpdateSessionNoteName").val(),
            sessionNoteBody: editorData,
            sessionNoteID: parseInt($("#sessionNoteFormId").val(), 10)
        }

        fetch("api/SessionNote/EditNote", {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(item)
        })
            .then(() => location.reload())
            .catch(() => console.log("Failed to update item"));

    });
}

function createNewSection() {

    item = {

        sessionSectionName: $("#inputNewSection").val(),
        campaignID: parseInt($("#inputCampaignID").val(), 10)
    }

    fetch("api/SessionSection", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(item)
    })
        .then((response) => response.json())
        .then(() => location.reload())
        .catch(() => console.log("Failed to create item"));
}

function createNewNote(sessionSectionID) {

    item = {

        sessionNoteTitle: $("#inputNewNote" + sessionSectionID).val(),
        campaignID: parseInt($("#inputCampaignID").val(), 10),
        sessionSectionID: parseInt(sessionSectionID, 10)
    }

    fetch("api/SessionNote", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(item)
    })
        .then((response) => response.json())
        .then(() => location.reload())
        .catch(() => console.log("Failed to create item"));
}

function selectSessionNote(sessionNoteID) {

    fetch("api/SessionNote/" + sessionNoteID, {
        method: 'PUT',
    })
        .then(() => location.reload())
        .catch(() => console.log("Failed to update item"));

}