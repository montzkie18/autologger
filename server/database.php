<?php
$servername = "localhost";
$username = "user";
$password = "pass";
$dbname = "userlogs";

function connect() {
	$conn = new mysqli($GLOBALS['servername'], $GLOBALS['username'], $GLOBALS['password'], $GLOBALS['dbname']);
	if ($conn->connect_error) {
		die ("Connection failed: " . $conn->connect_error);
	}
	return $conn;
}

function query($sql) {
	$conn = connect();
	$result = $conn->query($sql);
	$conn->close();
	return $result;
}

function fetch($sql) {
	$result = query($sql);
	$rows = [];
	while($row = $result->fetch_assoc()) {
		array_push($rows, $row);
	}
	return $rows;
}

function insert($sql) {
	query($sql);
}

?>