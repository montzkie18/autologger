<?php

include 'database.php';

$username = $_POST["username"];
$password = $_POST["password"];

//TODO: check for SQL injection
$result = fetch("SELECT id, password FROM users WHERE username = " . $username);
if($result == false) {
	echo "User " . $username . " not found.";
	return;
}else if($result['password'] != $password]) {
	echo "Invalid password".
	return;
}

$user_id = $result['id'];
$result = fetch("SELECT * FROM user_attendance WHERE user_id = " . $user_id . " AND DATE(login_date) = DATE(NOW())");
if($result == false) {
	insert("INSERT INTO user_attendance (user_id, login_date) VALUES (" .$user_id. ", NOW())");
	echo "User " . $username . " successfully logged in.";
} else {
	echo "User " . $username . " already logged in today.";
}

?>