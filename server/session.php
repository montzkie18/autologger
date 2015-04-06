<?php

session_start();

if(isset($_SESSION["timeout"])) {
	if($_SESSION["timeout"] + 10 * 60 < time()) {
		session_unset();
		session_destroy();
		Header("Location: signin.php");
	}
}

?>