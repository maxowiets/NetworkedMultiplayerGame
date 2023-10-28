<?php
include "Connection.php";

$query = "SELECT * FROM games"; //query

if (!($result = $mysqli->query($query))) { // query toepassen
    showerror($mysqli->errno, $mysqli->error); // als toepassen mislukt error laten zien
}

//for 1 result
$row = $result->fetch_assoc(); //info uit "brei" halen
echo "RETURN FIRST VALUE<br>";
echo json_encode($row); //json laten zien
echo "test";
echo "<br><br>";

//for 2 results
$my_json = "{\"users\":["; //aanmaken variabele die hele json gaat bevatten en beginvulling van json
$row = $result->fetch_assoc(); //haal eerste row uit "brei"

do { //begin van de loop om alle rows uit result te halen
    $my_json .= json_encode($row); //row omzetten naar json en toevoegen aan variabele
} while ($row = $result->fetch_assoc()); // einde loop

$my_json .= "]}"; // afsluiting json
echo "RETURN WHOLE COLUMN<br>";
echo $my_json; // json laten zien
echo "<br><br>";

//GET MAX VALUE FROM COLUMN
$query2 = "SELECT * FROM games WHERE id = (SELECT MAX(id) FROM games)";
if (!($result = $mysqli->query($query2))) {
    showerror($mysqli->errno, $mysqli->error);
}
$row2 = $result->fetch_assoc();
echo "RETURN HIGHEST VALUE FROM COLUMN<br>";
echo json_encode($row2);
echo "<br><br>";

//GET VALUE FROM URL
$newName = $_GET["name"];
$newPassword = $_GET["password"];
echo "RETURN VALUE FROM URL<br>";
if ($mysqli->real_escape_string($newName)) {
    echo $newName;
}
echo "<br><br>";

//INSERT INTO TABLE
if ($newName != null && $newPassword != null) {
    $query3 = "INSERT INTO players (id, name, password) VALUES (NULL, '$newName', '$newPassword')";
    if (!($result = $mysqli->query($query3))) {
        showerror($mysqli->errno, $mysqli->error);
    }
}

?>