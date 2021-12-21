<?php
    $servername = "localhost";
    $username = "nelsazdu_metalraiders01";
    $password = "pHIN[tr!oRnQ";
    $dbname = "nelsazdu_metalraiders_global";
    
    $conn = new mysqli($servername, $username, $password, $dbname);

    if($_POST["service"] == "register"){


        $username = $_POST["username"];
        $password = hash('sha512', $_POST["password"]);
        
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
        $password = hash('sha512', $_POST["password"]);
        
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
        }else{
            echo "wrong credentials";
        }

        $conn->close();
    }
    
    
    
    if($_POST["service"] == "getBasicPvPStats"){


        $playerId = $_POST["playerId"];
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        $sql = "SELECT player_id, kills, deaths, headshots FROM player_basic_pvp_stats WHERE player_id='$playerId'";
        $result = $conn->query($sql);

        if($result->num_rows > 0){
            $row = array();

            while($row = $result->fetch_assoc()){
                $rows[] = $row;
            }
            // Make sure no other echo or unity will read the json string and otther merges as a single string
            echo json_encode($rows[0]);

        }else{
            echo "Could not fetch pvp stats. SQL request: '$sql'";
        }

        $conn->close();
    }
    
    
    
    if($_POST["service"] == "getBasicPvEStats"){


        $playerId = $_POST["playerId"];
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        $sql = "SELECT player_id, kills, deaths, headshots, total_points FROM player_basic_pve_stats WHERE player_id='$playerId'";
        $result = $conn->query($sql);

        if($result->num_rows > 0){
            $row = array();

            while($row = $result->fetch_assoc()){
                $rows[] = $row;
            }
            // Make sure no other echo or unity will read the json string and otther merges as a single string
            echo json_encode($rows[0]);

        }else{
            echo "Could not fetch pve stats. SQL request: '$sql'";
        }

        $conn->close();
    }
    
    
    
    if($_POST["service"] == "SaveSwarmStats"){


        $playerId = $_POST["playerId"];
        $username = $_POST["username"];
        $newKills = $_POST["newKills"];
        $newDeaths = $_POST["newDeaths"];
        $newHeadshots = $_POST["newHeadshots"];
        $newTotalPoints = $_POST["newTotalPoints"];
        
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        try{
            $sql = "UPDATE player_basic_pve_stats SET kills='$newKills', deaths='$newDeaths', headshots='$newHeadshots', total_points='$newTotalPoints' WHERE player_id='$playerId'";
            $result = $conn->query($sql);
            echo "Swarm stats saved successfully";
        }catch(Exception $e){
            echo "Could not save swarm stats. SQL request: '$sql'";
        }

        $conn->close();
    }
    
    
    if($_POST["service"] == "SaveMultiplayerStats"){


        $playerId = $_POST["playerId"];
        $username = $_POST["username"];
        $newKills = $_POST["newKills"];
        $newDeaths = $_POST["newDeaths"];
        $newHeadshots = $_POST["newHeadshots"];
        
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        try{
            $sql = "UPDATE player_basic_pvp_stats SET kills='$newKills', deaths='$newDeaths', headshots='$newHeadshots' WHERE player_id='$playerId'";
            $result = $conn->query($sql);
            echo "Swarm stats saved successfully";
        }catch(Exception $e){
            echo "Could not save multiplayer stats. SQL request: '$sql'";
        }

        $conn->close();
    }
?>