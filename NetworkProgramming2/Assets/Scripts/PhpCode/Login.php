<?php
require "Connection.php";
if (isset($_POST['username']) && isset($_POST['password'])) {
    $errors = array();

    $username = $_POST['username'];
    $password = $_POST['password'];
    if ($stmt = $mysqli->prepare("SELECT id, password FROM players WHERE username = ?")) {


        /* bind parameters for markers */
        $stmt->bind_param('s', $username);

        /* execute query */
        if ($stmt->execute()) {

            /* store result */
            $stmt->store_result();

            if ($stmt->num_rows > 0) {
                /* bind result variables */
                $stmt->bind_result($id_tmp, $password_hash);

                /* fetch value */
                $stmt->fetch();

                if (password_verify($password, $password_hash)) {
                    echo "Success" . "|" . $id_tmp;

                    return;
                } else {
                    $errors[] = "Wrong username or password hash.";
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