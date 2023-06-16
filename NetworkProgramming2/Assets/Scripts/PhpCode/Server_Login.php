<?php
include "Connection.php";
session_start();
echo $_SESSION["server_id"] = $_GET["id"];
echo $_SESSION["user_id"];
echo "<br>";
echo session_id();

?>