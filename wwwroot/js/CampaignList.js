const CampaignUri = "api/Campaigns/";
//const formData = new FormData();

function CreateCampaign() {

    item = {

        campaignName: $("#inputCampaignName").val(),

    }

    fetch(CampaignUri, {
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

function displayEditCampaign() {

    $("#campaignBody").toggleClass("d-none");
    $("#areaEditCampaign").toggleClass("d-none");
    let currentCampaignDescription = $("#campaignDescription").html();
    $("#textAreaCampaignDescription").html(currentCampaignDescription);
    attachEditor();

}

function attachEditor() {

    let editor;

    ClassicEditor
        .create(document.querySelector('#textAreaCampaignDescription'))
        .then(newEditor => {
            editor = newEditor;
        })
        .catch(error => {
            console.error(error);
        });

    let nameInput = $("#inputEditCampaignName");
    let campaignName = $("#campaignName");

    nameInput.toggleClass("d-none")
    campaignName.toggleClass("d-none");
    nameInput.val(campaignName.text());

    $("#btnUpdateCampaign").on("click", async function (evt) {
        let editorData = editor.getData();

        evt.preventDefault();

        item = {

            campaignID: parseInt($("#selectedCampaignID").val(), 10),
            campaignName: $("#inputEditCampaignName").val(),
            campaignDescription: editorData,
        }

        fetch("api/Campaigns", {
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


function selectCampaign(id) {


    fetch(CampaignUri + id, {
        method: 'PUT',
    })
        .then(() => location.reload())
        .catch(() => console.log("Failed to update item"));
}

function deleteCampaign(id) {

    fetch(CampaignUri + id, {
        method: 'DELETE'
    })
        .then(() => location.reload())
        .catch(() => console.log("Failed to delete item"));

}