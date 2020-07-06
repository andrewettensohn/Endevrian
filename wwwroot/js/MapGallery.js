function ActivateTag(mapId, tagId) {

    item = {
        tagID: tagId,
        mapID: mapId
    }

    fetch("api/Tag/Relate", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(item)
    })
        .catch(() => console.log("Failed to create item"));

}

function DeactivateTag(tagRelationId) {

    fetch("api/Tag/Relate/" + tagRelationId, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .catch(() => console.log("Failed to update item"));
}

function deleteMap(mapId) {

    //TODO: Don't ask me for confirmation again
    fetch("api/Map/" + mapId, {
        method: 'DELETE',
    })
        .then(() => window.location.reload());

}

function InputNewMapName(mapId) {

    $("#headerMapName" + mapId).toggleClass("d-none");

    var inputMap = $("#inputMapName" + mapId);
    $("#inputGroupMapName").toggleClass("d-none");

    $("#btnSubmitMapNameChange").on("click", async function (evt) {
        evt.preventDefault();

        var newMapName = inputMap.val();

        fetch("api/Map/" + mapId + "/" + newMapName, {
            method: 'PUT',
        })
            .then(() => window.location.reload());

    });
}