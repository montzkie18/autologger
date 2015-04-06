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
//TODO: might want to include logins from previous day incase of OT til 12am
$result = fetch("SELECT * FROM user_attendance " .
				"WHERE user_id = " . $user_id . 
				" AND logout_date = NULL " .
				" AND DATE(login_date) = DATE(NOW())");
				
if($result == false) {
	echo "User " . $username . " either already logged out or did not login yet.";
} else {
	query(	"UPDATE user_attendance " .
			"SET logout_date = NOW() " .
			"WHERE user_id = " . $user_id . 
			" AND DATE(login_date) = DATE(NOW())");
	echo "Logged out successfully";
}

?>