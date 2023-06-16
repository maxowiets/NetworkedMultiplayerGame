<?php
   $db_user = 'maxymebeling';
   $db_pass = 'kie8teethiTe';
   $db_host = 'localhost';
   $db_name = 'maxymebeling';

/* Open a connection */
$mysqli = new mysqli("$db_host","$db_user","$db_pass","$db_name");

/* check connection */
if ($mysqli->connect_errno) {
   echo "Failed to connect to MySQL: (" . $mysqli->connect_errno() . ") " . $mysqli->connect_error();
   exit();
}

function showerror($error,$errornr) {
die("Error (" . $errornr . ") " . $error);
}
?>