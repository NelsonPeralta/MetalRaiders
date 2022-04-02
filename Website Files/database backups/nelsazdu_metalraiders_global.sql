-- phpMyAdmin SQL Dump
-- version 4.9.7
-- https://www.phpmyadmin.net/
--
-- Host: localhost:3306
-- Generation Time: Apr 02, 2022 at 01:57 PM
-- Server version: 10.3.34-MariaDB-log-cll-lve
-- PHP Version: 7.3.32

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `nelsazdu_metalraiders_global`
--

-- --------------------------------------------------------

--
-- Table structure for table `player_basic_global_data`
--

CREATE TABLE `player_basic_global_data` (
  `player_id` int(128) NOT NULL,
  `level` int(128) NOT NULL DEFAULT 1,
  `xp` int(128) NOT NULL DEFAULT 0,
  `credits` int(128) NOT NULL DEFAULT 0
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

--
-- Dumping data for table `player_basic_global_data`
--

INSERT INTO `player_basic_global_data` (`player_id`, `level`, `xp`, `credits`) VALUES
(31, 3, 3459, 3459);

-- --------------------------------------------------------

--
-- Table structure for table `player_basic_pve_stats`
--

CREATE TABLE `player_basic_pve_stats` (
  `player_id` int(11) NOT NULL,
  `kills` int(11) NOT NULL DEFAULT 0,
  `deaths` int(11) NOT NULL DEFAULT 0,
  `headshots` int(11) NOT NULL DEFAULT 0,
  `highest_points` int(11) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

--
-- Dumping data for table `player_basic_pve_stats`
--

INSERT INTO `player_basic_pve_stats` (`player_id`, `kills`, `deaths`, `headshots`, `highest_points`) VALUES
(29, 0, 0, 0, 0),
(28, 0, 0, 0, 0),
(27, 142, 18, 76, 114568),
(30, 4, 4, 0, 1088),
(31, 34, 1, 17, 2226);

-- --------------------------------------------------------

--
-- Table structure for table `player_basic_pvp_stats`
--

CREATE TABLE `player_basic_pvp_stats` (
  `player_id` int(11) NOT NULL,
  `kills` int(11) NOT NULL DEFAULT 0,
  `deaths` int(11) NOT NULL DEFAULT 0,
  `headshots` int(11) NOT NULL DEFAULT 0
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

--
-- Dumping data for table `player_basic_pvp_stats`
--

INSERT INTO `player_basic_pvp_stats` (`player_id`, `kills`, `deaths`, `headshots`) VALUES
(31, 12, 17, 2),
(30, 3, 2, 1),
(29, 0, 0, 0),
(28, 0, 0, 0),
(27, 22, 18, 8);

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `username` varchar(64) NOT NULL,
  `password` varchar(1000) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `username`, `password`) VALUES
(27, 'Nelson', 'bf9645d128aefb77e22b12c12d396fd0baca19f014201b5c3be3866c460367584ba956a2327132c9704b7bc12be6d892b45ee1726bb9a3fcfd5553bd98695e6f'),
(28, 'Tensei', 'ccb6cfe12559a73bb7cbd7ef2d46ee635f85ca2a60e4e82c2836ebc9b266eb12fd61f1afd64b3a2f22a083f579722757cb10cebaab5f0cf14c00017a457f5784'),
(29, 'Test', 'c6ee9e33cf5c6715a1d148fd73f7318884b41adcb916021e2bc0e800a5c5dd97f5142178f6ae88c8fdd98e1afb0ce4c8d2c54b5f37b30b7da1997bb33b0b8a31'),
(30, 'q', '2e96772232487fb3a058d58f2c310023e07e4017c94d56cc5fae4b54b44605f42a75b0b1f358991f8c6cbe9b68b64e5b2a09d0ad23fcac07ee9a9198a745e1d5'),
(31, 'a', '1f40fc92da241694750979ee6cf582f2d5d7d28e18335de05abc54d0560e0f5302860c652bf08d560252aa5e74210546f369fbbbce8c12cfc7957b2652fe9a75');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `player_basic_pve_stats`
--
ALTER TABLE `player_basic_pve_stats`
  ADD UNIQUE KEY `player_id` (`player_id`);

--
-- Indexes for table `player_basic_pvp_stats`
--
ALTER TABLE `player_basic_pvp_stats`
  ADD UNIQUE KEY `player_id` (`player_id`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `username` (`username`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=32;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
