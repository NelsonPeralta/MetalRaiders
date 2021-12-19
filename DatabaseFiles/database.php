<?php
    $servername = "localhost";
    $username = "nelsazdu_metalraiders01";
    $password = "pHIN[tr!oRnQ";
    $dbname = "nelsazdu_metalraiders_global";
    
    $conn = new mysqli($servername, $username, $password, $dbname);

    if($_POST["service"] == "register"){


        $username = $_POST["username"];
        $username =substr_replace($username ,"",-3);
        //$password = hash('sha512', $_POST["password"]);
        $password = hash('sha512', substr_replace($_POST["password"], "", -3));
        //$password = $_POST["password"];
        //$password = substr_replace($_POST["password"], "", -3);
        
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
        
        $sql = "INSERT INTO player_basic_pvp_stats(player_id) VALUES ((SELECT id FROM users WHERE username='$username'))";

        $result = $conn->query($sql);
        if($result === TRUE){

            echo "Successfully added Player Basic PvP Stats";
        }else{
            echo "error: " . $sql . "<br>" . $conn->error;
        }
        
        
        $sql = "INSERT INTO player_basic_pve_stats(player_id) VALUES ((SELECT id FROM users WHERE username='$username'))";

        $result = $conn->query($sql);
        if($result === TRUE){

            echo "Successfully added Player Basic PvE Stats";
        }else{
            echo "error: " . $sql . "<br>" . $conn->error;
        }
        
        

        $conn->close();
    }

    if($_POST["service"] == "login"){


        $username = $_POST["username"];
        $username =substr_replace($username ,"",-3);
        //$password = hash('sha512', $_POST["password"]);
        $password = hash('sha512', substr_replace($_POST["password"], "", -3));
        //$password = $_POST["password"];
        //$password = substr_replace($_POST["password"], "", -3);
        
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
    
    
    
    if($_POST["service"] == "getBasicPvPStats"){


        $username = $_POST["username"];
        $username =substr_replace($username ,"",-3);
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        $sql = "SELECT player_id, kills, deaths, headshots FROM player_basic_pvp_stats WHERE player_id=(SELECT id FROM users WHERE username='$username')";
        $result = $conn->query($sql);

        if($result->num_rows > 0){
            $row = array();

            while($row = $result->fetch_assoc()){
                $rows[] = $row;
            }
            // Make sure no other echo or unity will read the json string and otther merges as a single string
            echo json_encode($rows[0]);

        }else{
            echo "Could not fetch pvp stats";
        }

        $conn->close();
    }
    
    
    
    if($_POST["service"] == "getBasicPvEStats"){


        $username = $_POST["username"];
        $username =substr_replace($username ,"",-3);
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        $sql = "SELECT player_id, kills, deaths, headshots FROM player_basic_pve_stats WHERE player_id=(SELECT id FROM users WHERE username='$username')";
        $result = $conn->query($sql);

        if($result->num_rows > 0){
            $row = array();

            while($row = $result->fetch_assoc()){
                $rows[] = $row;
            }
            // Make sure no other echo or unity will read the json string and otther merges as a single string
            echo json_encode($rows[0]);

        }else{
            echo "Could not fetch pve stats";
        }

        $conn->close();
    }
?>