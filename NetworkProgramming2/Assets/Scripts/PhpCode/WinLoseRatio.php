<?php
require "Connection.php";
if (isset($_POST['session_id']) && isset($_POST['game_id']) && isset($_POST['player_id'])) {
    session_id($_POST['session_id']);
    session_start();
    if ($_SESSION['server_id'] != NULL && $_SESSION['server_id'] != "" && $_SESSION['server_id'] != 0) {
        $game_id = $_POST['game_id'];
        $player_id = $_POST['player_id'];
        $result1 = $mysqli->query("SELECT winner_id, COUNT(*) as c FROM played_games WHERE game_id = '{$game_id}' AND winner_id = '{$player_id}'");
        $row1 = $result1->fetch_row();
        $result2 = $mysqli->query("SELECT loser_id, COUNT(*) as c FROM played_games WHERE game_id = '{$game_id}' AND loser_id = '{$player_id}'");
        $row2 = $result2->fetch_row();
        $result3 = $mysqli->query("SELECT COUNT(*) as c FROM played_games WHERE game_id = '{$game_id}' AND date >= DATE_SUB(CURDATE(), INTERVAL 1 MONTH)");
        $row3 = $result3->fetch_row();
        $winLoseResponse = new CreateWinLoseRatio($row1[1], $row2[1], $row3[0]);
        echo json_encode($winLoseResponse);
    } else {
        echo "Session doesn't exist";
    }

} else {
    echo "Missing data";
}

class CreateWinLoseRatio
{
    public $wins;
    public $loses;
    public $totalGames;
    function __construct($wins, $loses, $totalGames)
    {
        $this->wins = $wins;
        $this->loses = $loses;
        $this->totalGames = $totalGames;
    }
}
?>