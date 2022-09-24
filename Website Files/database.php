<?php
    $servername = "localhost";
    $username = "nelsazdu_metalraiders01";
    $password = "pHIN[tr!oRnQ";
    $dbname = "nelsazdu_metalraiders_global";
    
    $conn = new mysqli($servername, $username, $password, $dbname);

    if($_POST["service"] == "register"){


        $username = $_POST["username"];
        //$username = substr_replace($username ,"",-3);
        $password = hash('sha512', $_POST["password"]);
        //$password = hash('sha512', substr_replace($_POST["password"], "", -3));
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
        //$username =substr_replace($username ,"",-3);
        $password = hash('sha512', $_POST["password"]);
        //$password = hash('sha512', substr_replace($_POST["password"], "", -3));
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
    
    
    
    if($_POST["service"] == "getBasicOnlineData"){


        $playerId = $_POST["playerId"];
        //$username =substr_replace($username ,"",-3);
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        $sql = "SELECT player_id, level, xp, credits, armor_data_string, armor_data_string, unlocked_armor_data_string FROM player_basic_global_data WHERE player_id='$playerId'";
        $result = $conn->query($sql);

        if($result->num_rows > 0){
            $row = array();

            while($row = $result->fetch_assoc()){
                $rows[] = $row;
            }
            // Make sure no other echo or unity will read the json string and otther merges as a single string
            echo json_encode($rows[0]);

        }else{
            echo "Could not fetch basic global data. SQL request: '$sql'";
        }

        $conn->close();
    }
    
    
    
    if($_POST["service"] == "getBasicPvPStats"){


        $playerId = $_POST["playerId"];
        //$username =substr_replace($username ,"",-3);
        
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
        //$username =substr_replace($username ,"",-3);
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        $sql = "SELECT player_id, kills, deaths, headshots, highest_points FROM player_basic_pve_stats WHERE player_id='$playerId'";
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
    
    
    
    
    
    
    if($_POST["service"] == "SaveBasicOnlineStats"){


        $playerId = $_POST["playerId"];
        $newLevel = $_POST["newLevel"];
        $newXp = $_POST["newXp"];
        $newCredits = $_POST["newCredits"];
        
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        try{
            $sql = "UPDATE player_basic_global_data SET level='$newLevel', xp='$newXp', credits='$newCredits' WHERE player_id='$playerId'";
            $result = $conn->query($sql);
            echo "Swarm stats saved successfully";
        }catch(Exception $e){
            echo "Could not save swarm stats. SQL request: '$sql'";
        }

        $conn->close();
    }
    
    
    if($_POST["service"] == "SaveSwarmStats"){


        $playerId = $_POST["playerId"];
        $username = $_POST["username"];
        $newKills = $_POST["newKills"];
        $newDeaths = $_POST["newDeaths"];
        $newHeadshots = $_POST["newHeadshots"];
        $newHighestPoints = $_POST["newHighestPoints"];
        
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        try{
            $sql = "UPDATE player_basic_pve_stats SET kills='$newKills', deaths='$newDeaths', headshots='$newHeadshots', highest_points='$newHighestPoints' WHERE player_id='$playerId'";
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
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    if($_POST["service"] == "SaveUnlockedArmorStringData"){

        $playerId = $_POST["playerId"];
        $newUnlockedArmorStringData = $_POST["newUnlockedArmorStringData"];
        $newPlayerCredits = $_POST["newPlayerCredits"];
        
        
        if($conn->connect_error){
            die("Connection failed: " . $conn->connect_error);
        }
        
        try{
            $sql = "UPDATE player_basic_global_data SET unlocked_armor_data_string='$newUnlockedArmorStringData', credits='$newPlayerCredits' WHERE player_id='$playerId'";
            $result = $conn->query($sql);
            echo "UnlockedArmorStringData saved successfully";
        }catch(Exception $e){
            echo "Could not save UnlockedArmorStringData. SQL request: '$sql'";
        }

        $conn->close();
    }
?>