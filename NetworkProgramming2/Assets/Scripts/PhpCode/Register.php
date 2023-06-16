<?php
require "Connection.php";

if (isset($_POST["email"]) && isset($_POST["username"]) && isset($_POST["password"]) && isset($_POST["confirmPassword"])) {
    $errors = array();

    $emailMaxLength = 254;
    $usernameMaxLength = 20;
    $usernameMinLength = 3;
    $passwordMaxLength = 25;
    $passwordMinLength = 5;

    $email = strtolower($_POST["email"]);
    $username = $_POST["username"];
    $password = $_POST["password"];
    $confirmPassword = $_POST["confirmPassword"];

    //Validate email
    if (preg_match('/\s/', $email)) {
        $errors[] = "Email can't have spaces";
    } else {
        if (!validate_email_address($email)) {
            $errors[] = "Invalid email";
        } else {
            if (strlen($email) > $emailMaxLength) {
                $errors[] = "Email is too long, must be equal or under " . strval($emailMaxLength) . " characters";
            }
        }
    }

    //Validate username
    if (strlen($username) > $usernameMaxLength || strlen($username) < $usernameMinLength) {
        $errors[] = "Incorrect username length, must be between " . strval($usernameMinLength) . " and " . strval($usernameMaxLength) . " characters";
    } else {
        if (!ctype_alnum($username)) {
            $errors[] = "Username must be alphanumeric";
        }
    }

    //Validate password
    if ($password != $confirmPassword) {
        $errors[] = "Passwords do not match";
    } else {
        if (preg_match('/\s/', $password)) {
            $errors[] = "Password can't have spaces";
        } else {
            if (strlen($password) > $passwordMaxLength || strlen($password) < $passwordMinLength) {
                $errors[] = "Incorrect password length, must be between " . strval($passwordMinLength) . " and " . strval($passwordMaxLength) . " characters";
            } else {
                if (!preg_match('/[A-Za-z]/', $password) || !preg_match('/[0-9]/', $password)) {
                    $errors[] = "Password must contain atleast 1 letter and 1 number";
                }
            }
        }
    }

    //Check if there is user already registered with the same email or username
    if (count($errors) == 0) {
        if ($stmt = $mysqli->prepare("SELECT username, email FROM players WHERE email = ? OR username = ? LIMIT 1")) {

            /* bind parameters for markers */
            $stmt->bind_param('ss', $username, $email);

            /* execute query */
            if ($stmt->execute()) {

                /* store result */
                $stmt->store_result();

                if ($stmt->num_rows > 0) {

                    /* bind result variables */
                    $stmt->bind_result($username_tmp, $email_tmp);

                    /* fetch value */
                    $stmt->fetch();

                    if ($email_tmp == $email) {
                        $errors[] = "User with this email already exist.";
                    } else if ($username_tmp == $username) {
                        $errors[] = "User with this name already exist.";
                    }
                }

                /* close statement */
                $stmt->close();

            } else {
                $errors[] = "Something went wrong, please try again.";
            }
        } else {
            $errors[] = "Something went wrong, please try again.";
        }
    }

    //Finalize registration
    if (count($errors) == 0) {
        $hashedPassword = password_hash($password, PASSWORD_BCRYPT);
        if ($stmt = $mysqli->prepare("INSERT INTO players (username, email, password) VALUES(?, ?, ?)")) {

            /* bind parameters for markers */
            $stmt->bind_param('sss', $username, $email, $hashedPassword);

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
    }
    if (count($errors) > 0) {
        echo $errors[0];
    } else {
        echo json_encode("Success");
    }
} else {
    echo "Missing data";
}

function validate_email_address($email)
{
    return preg_match('/^([a-z0-9!#$%&\'*+-\/=?^_`{|}~.]+@[a-z0-9.-]+\.[a-z0-9]+)$/i', $email);
}
?>