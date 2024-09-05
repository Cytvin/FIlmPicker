function updateRoomData() {
    $.ajax(
        {
            method: 'get',
            url: 'Invitations',
            success: function (result) {
                $('#roomData').empty();
                $('#roomData').html(result);
            }
        })
}

function getRoomData(roomId) {
    $.ajax(
        {
            method: 'get',
            url: 'Details/' + roomId,
            success: function (result) {
                $('#roomData').empty();
                $('#roomData').html(result);
            }
        }
    )
}

function getMatches(roomId) {
    $.ajax(
        {
            method: 'get',
            url: 'Matches/' + roomId,
            success: function (result) {
                $('#roomData').empty();
                $('#roomData').html(result);
            }
        }
    )
}

function getSettings(roomId) {
    $.ajax(
        {
            method: 'get',
            url: 'Settings/' + roomId,
            success: function (result) {
                $('#roomData').empty();
                $('#roomData').html(result);
            }
        }
    )
}

function likeMovie() {
    sendScore('Like');
}

function dislikeMovie() {
    sendScore('Dislike');
}

function sendScore(score) {
    var form = $('#scoreMovieForm').get(0);
    var url = form.getAttribute('action');

    var scoreInput = document.createElement('input');
    scoreInput.setAttribute('type', 'hidden');
    scoreInput.setAttribute('name', 'score');
    scoreInput.setAttribute('value', score);

    form.appendChild(scoreInput);

    $.ajax(
        {
            method: 'post',
            url: url,
            data: $(form).serialize(),
            contentType: 'application/x-www-form-urlencoded; charset=utf-8',
            success: function (result) {
                $('#roomData').empty();
                $('#roomData').html(result);
            }
        }
    )
}

$(document).on('input', '#minRating', function () {
    $('#minRatingValue').html($(this).val());
});

$(document).on('input', '#maxRating', function () {
    $('#maxRatingValue').html($(this).val());
});

function saveSettings() {
    var form = $('#setSettings').get(0);
    var url = form.getAttribute('action');

    $.ajax(
        {
            method: 'post',
            url: url,
            data: $(form).serialize(),
            contentType: 'application/x-www-form-urlencoded; charset=utf-8',
            success: function (result) {
                $('#roomData').empty();
                $('#roomData').html(result);
            }
        }
    )
}