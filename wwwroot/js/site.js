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
    inputMap.toggleClass("d-none");

    inputMap.on("keypress focusout", async function (evt) {
        var key = evt.which;
        if (key == 13 || key == 0) {

            evt.preventDefault();

            var newMapName = inputMap.val();

            fetch("api/Map/" + mapId + "/" + newMapName, {
                method: 'PUT',
            })
                .then(() => window.location.reload());
        }
    });
}