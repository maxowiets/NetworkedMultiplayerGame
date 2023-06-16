<?php
require "Connection.php";
if (isset($_POST['game_id']) && isset($_POST['winner_id']) && isset($_POST['loser_id'])) {
    $errors = array();
    if ($stmt = $mysqli->prepare("INSERT INTO played_games (game_id, winner_id, loser_id) VALUES (?, ?, ?)")) {

        $game_id = $_POST['game_id'];
        $winner_id = $_POST['winner_id'];
        $loser_id = $_POST['loser_id'];
         
        /* bind parameters for markers */
        $stmt->bind_param('iii', $game_id, $winner_id, $loser_id);

        /* execute query */
        if ($stmt->execute()) {

            /* close statement */
            $stmt->close();

        } else {
            $errors[] = "Something went wrong, please try again.";
        }
    } else {
        $errors[] = "Something went wrong, please try again.";
    }

    if (count($errors) > 0) {
        echo $errors[0];
    }
} else {
    echo "Missing data";
}
?>