function refreshPlayers() {
    var playerSelect = $("#player-select");
    playerSelect.empty();

    $.getJSON("/get/players").done(function (players) {
        for (var i = 0; i < players.length; i++) {
            var option = $("<option>");
            option.val(players[i]);
            option.text(players[i]);

            playerSelect.append(option);
        }
    });
}

$(function() {
    var jsEditor = ace.edit("js-editor");
    jsEditor.setTheme("ace/theme/monokai");
    jsEditor.getSession().setMode("ace/mode/javascript");

    $("#run-js").click(function (e) {
        e.preventDefault();

        console.log("asdf");

        var data = JSON.stringify({ Code: jsEditor.getValue(), TargetPlayer: $("#player-select").val() });
        $.post("/post/client", data);
    });

    $("#refresh-players").click(function (e) {
        e.preventDefault();

        refreshPlayers();
    });

    refreshPlayers();
});