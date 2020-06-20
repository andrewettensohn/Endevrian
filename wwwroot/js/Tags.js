$("#newTagForm").on("submit", async function (evt) {

    evt.preventDefault();
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

});

function UpdateTagName(id) {

    var input = $("#inputEditTagName" + id);
    var header = $("#headerTagName" + id);

    header.toggleClass("d-none");
    input.val(header.text());
    input.toggleClass("d-none");

    input.on("keypress focusout", async function (evt) {
        var key = evt.which;
        if (key == 13 || key == 0) {

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
        }

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