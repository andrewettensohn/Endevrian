function AddTag() {

    var tagName = $("#inputNewTagName").val();
    fetch("api/Tag/" + tagName, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then((response) => response.json())
        .then(() => location.reload())
        .catch(() => console.log("Failed to create item"));
}

function UpdateTagName(id) {

    var inputGroup = $("#editInputGroup" + id);
    var input = $("#inputEditTagName" + id);
    var header = $("#headerTagName" + id);

    header.toggleClass("d-none");
    input.val(header.text());
    inputGroup.toggleClass("d-none");

    $("#btnSubmitEdit").on("click", async function (evt) {

        evt.preventDefault();

        item = {

            tagID: id,
            name: input.val()
        }

        fetch("api/Tag", {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(item)
        })
            .then((response) => response.json())
            .then(() => location.reload())
            .catch(() => console.log("Failed to update item"));
    });
}

function DeleteTag(id) {

    fetch("api/Tag/" + id, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then(() => location.reload())
        .catch(() => console.log("Failed to create item"));
}