<?php
    $servername = "localhost";
    $username = "nelsazdu_metalraiders01";
    $password = "pHIN[tr!oRnQ";
    $dbname = "nelsazdu_metalraiders_global";
    
    $conn = new mysqli($servername, $username, $password, $dbname);

    if($_POST["service"] == "register"){


        $username = $_POST["username"];
        $password = hash('sha512', $_POST["password"]);
        $password = $_POST["password"];
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        
        $sql = "INSERT INTO users(username, password) VALUES ('$username', '$password')";

        $result = $conn->query($sql);
        if($result === TRUE){

            echo "success2";
        }else{
            echo "error: " . $sql . "<br>" . $conn->error;
        }

        $conn->close();
    }

    if($_POST["service"] == "login"){


        $username = $_POST["username"];
        $password = hash('sha512', $_POST["password"]);
        $password = $_POST["password"];
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        
        // $sql = "SELECT password FROM users WHERE username='$username'";
        $sql = "SELECT id, username FROM users WHERE username='$username' AND password='$password'";

        $result = $conn->query($sql);

        if($result->num_rows > 0){
            $row = array();

            while($row = $result->fetch_assoc()){
                $rows[] = $row;
            }
            // Make sure no other echo or unity will read the json string and otther merges as a single string
            echo json_encode($rows[0]);


            // while($row = $result->fetch_assoc()){
            //     if($row["password"] == $password){
            //         echo "login success";
            //     }else{
            //         echo "wrong credentials";
            //     }
            // }



        }else{
            echo "wrong credentials";
            // echo "Username does not exist";
        }

        $conn->close();
    }
?>