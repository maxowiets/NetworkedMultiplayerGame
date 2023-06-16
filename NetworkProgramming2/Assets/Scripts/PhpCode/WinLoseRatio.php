<?php
require "Connection.php";
if (isset($_POST['game_id']) && isset($_POST['player_id'])) {
    $game_id = $_POST['game_id'];
    $player_id = $_POST['player_id'];
    $result1 = $mysqli->query("SELECT winner_id, COUNT(*) as c FROM played_games WHERE game_id = '{$game_id}' AND winner_id = '{$player_id}'");
    $row1 = $result1->fetch_row();
    $result2 = $mysqli->query("SELECT loser_id, COUNT(*) as c FROM played_games WHERE game_id = '{$game_id}' AND loser_id = '{$player_id}'");
    $row2 = $result2->fetch_row();
    $winLoseResponse = new CreateWinLoseRatio($row1[1] ,$row2[1]);
    echo json_encode($winLoseResponse);
} else {
    echo "Missing data";
}

class CreateWinLoseRatio
{
    public $wins;
    // .
    public $loses;
    function __construct($wins, $loses)
    {
        $this->wins = $wins;
        $this->loses = $loses;
    }
}
?>