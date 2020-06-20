const AdventureUri = "api/AdventureLogs/";

function AddLog() {

    ClassicEditor
        .create(document.querySelector('#textAreaLogBody'), {
            removePlugins: ['ImageUpload', 'MediaEmbed']
        })

    var logForm = $("#logForm");
    var formData = new FormData();

    logForm.on("submit", async function (evt) {

        evt.preventDefault();

        item = {

            logTitle: $("#inputLogTitle").val(),
            logBody: $("#textAreaLogBody").val(),
        }

        fetch(AdventureUri, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(item)
        })
            .then((response) => response.json())
            .then(() => location.reload())
            .catch(() => console.log("Failed to create item"));

    });
}

function deleteAdventureLog(id) {

    fetch(uri + id, {
        method: 'DELETE'
    })
        .then(() => location.reload())
        .catch(() => console.log("Failed to delete item"));

}

function displayEditLogName(id) {

    $("#hLogTitle" + id).toggleClass('d-none');
    var currentLogTitleText = $("#hLogTitle" + id).text();
    $("#inputLogTitle" + id).val(currentLogTitleText);
    $("#inputLogTitle" + id).toggleClass('d-none');
    $("#inputLogTitle" + id).focus();

}

function editLogName(id) {

    $("#inputLogTitle" + id).toggleClass('d-none');
    var updatedLogTitle = $("#inputLogTitle" + id).val();
    $("#hLogTitle" + id).toggleClass('d-none');
    $("#hLogTitle" + id).text(updatedLogTitle);

    item = {

        adventureLogID: id,
        logTitle: updatedLogTitle

    }

    fetch(AdventureUri + id, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(item)
    })
        .then((response) => response.json())
        .then(() => location.reload())
        .catch(() => console.log("Failed to update log name"));

}

function displayEditLogBody(id) {

    $("#pLogBody" + id).toggleClass('d-none');
    var currentLogBodyText = $("#pLogBody" + id).html();
    $("#areaInputLogBody" + id).html(currentLogBodyText);
    $("#areaInputLogBody" + id).toggleClass('d-none');
    $("#areaInputLogBody" + id).focus();
    $("#btnDeleteLog" + id).toggleClass('d-none');
    $("#btnUpdateLogBody" + id).toggleClass('d-none');
    attachEditor(id);

}

function attachEditor(id) {

    let editor;

    ClassicEditor
        .create(document.querySelector('#areaInputLogBody' + id))
        .then(newEditor => {
            editor = newEditor;
        })
        .catch(error => {
            console.error(error);
        });


    document.querySelector('#btnUpdateLogBody' + id).addEventListener('click', () => {
        const editorData = editor.getData();

        item = {

            adventureLogID: id,
            logBody: editorData

        }

        fetch(AdventureUri + id, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(item)
        })
            .then(() => location.reload())
            .catch(error => console.error('Unable to edit log body', error));

    });

}