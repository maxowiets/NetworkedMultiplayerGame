<?php
include "Connection.php";

if (isset($_POST['server_id']) && isset($_POST['password'])) {
    $errors = array();

    $server = $_POST['server_id'];
    $password = $_POST['password'];
    if ($stmt = $mysqli->prepare("SELECT id, password FROM gdv_server WHERE id = ?")) {


        /* bind parameters for markers */
        $stmt->bind_param('s', $server);

        /* execute query */
        if ($stmt->execute()) {

            /* store result */
            $stmt->store_result();

            if ($stmt->num_rows > 0) {
                /* bind result variables */
                $stmt->bind_result($id_tmp, $password_tmp);

                /* fetch value */
                $stmt->fetch();
                if ($password == $password_tmp) {
                    session_start();
                    $_SESSION["server_id"] = $id_tmp;
                    echo session_id();
                } else {
                    $errors[] = "Wrong username or password.";
                }
            } else {
                $errors[] = "Wrong username or password.";
            }

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