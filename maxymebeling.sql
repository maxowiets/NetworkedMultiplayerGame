-- phpMyAdmin SQL Dump
-- version 4.9.4
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Gegenereerd op: 16 jun 2023 om 15:12
-- Serverversie: 10.6.12-MariaDB
-- PHP-versie: 7.4.6

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `maxymebeling`
--

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `games`
--

CREATE TABLE `games` (
  `id` int(11) NOT NULL,
  `name` varchar(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Gegevens worden geëxporteerd voor tabel `games`
--

INSERT INTO `games` (`id`, `name`) VALUES
(1, 'MiniBall'),
(2, 'MaxiBall'),
(3, 'MiniBlox'),
(4, 'MaxiBlox'),
(5, 'ballBlox'),
(6, 'ShooterThingy');

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `gdv_server`
--

CREATE TABLE `gdv_server` (
  `id` int(11) NOT NULL,
  `name` varchar(20) NOT NULL,
  `password` varchar(25) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Gegevens worden geëxporteerd voor tabel `gdv_server`
--

INSERT INTO `gdv_server` (`id`, `name`, `password`) VALUES
(1, 'server1', '09ahw8h098hasiuedh'),
(2, 'server2', 'asdfhaterhasdfv34gd'),
(3, 'server3', '90hjf2938jfa90f'),
(4, 'server4', 'asd8ofgasdiufh3189'),
(5, 'server5', 'asidufbaiusdf');

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `played_games`
--

CREATE TABLE `played_games` (
  `id` int(11) NOT NULL,
  `date` datetime NOT NULL DEFAULT current_timestamp(),
  `game_id` int(11) NOT NULL,
  `winner_id` int(11) NOT NULL,
  `loser_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Gegevens worden geëxporteerd voor tabel `played_games`
--

INSERT INTO `played_games` (`id`, `date`, `game_id`, `winner_id`, `loser_id`) VALUES
(1, '2023-04-19 11:41:12', 3, 15, 0),
(2, '2023-04-19 11:41:12', 1, 4, 0),
(3, '2023-04-19 11:42:19', 2, 7, 0),
(4, '2023-04-19 11:42:19', 1, 4, 0),
(5, '2023-04-19 11:42:33', 1, 4, 0),
(6, '2023-06-15 18:17:58', 6, 4, 0),
(7, '2023-06-15 18:18:29', 6, 7, 0),
(8, '2023-06-15 19:29:00', 6, 1, 0),
(9, '2023-06-15 19:29:11', 6, 4, 0),
(10, '2023-06-15 19:29:18', 6, 7, 0),
(11, '2023-06-15 19:29:44', 6, 10, 0),
(12, '2023-06-15 19:35:29', 6, 8, 0),
(13, '2023-06-15 19:35:34', 6, 8, 0),
(14, '2023-06-15 19:35:39', 6, 8, 0),
(15, '2023-06-15 22:31:00', 1, 1, 1),
(16, '2023-06-15 22:33:30', 6, 7, 8),
(17, '2023-06-15 22:33:35', 6, 8, 7),
(18, '2023-06-15 23:19:11', 6, 8, 7),
(19, '2023-06-15 23:20:56', 6, 8, 7),
(20, '2023-06-15 23:22:33', 6, 8, 7),
(21, '2023-06-15 23:23:32', 6, 8, 7),
(22, '2023-06-15 23:25:25', 6, 7, 8),
(23, '2023-06-15 23:31:29', 6, 8, 7),
(24, '2023-06-15 23:32:07', 6, 8, 7),
(25, '2023-06-15 23:32:59', 6, 8, 7),
(26, '2023-06-15 23:36:36', 6, 7, 8),
(27, '2023-06-15 23:37:01', 6, 7, 8),
(28, '2023-06-15 23:37:55', 6, 8, 7),
(29, '2023-06-15 23:38:05', 6, 8, 7),
(30, '2023-06-15 23:38:13', 6, 8, 7),
(31, '2023-06-15 23:38:19', 6, 7, 8),
(32, '2023-06-16 12:02:43', 6, 8, 7),
(33, '2023-06-16 12:02:55', 6, 7, 8),
(34, '2023-06-16 12:03:06', 6, 7, 8),
(35, '2023-06-16 12:03:15', 6, 8, 7),
(36, '2023-06-16 12:03:35', 6, 8, 7),
(37, '2023-06-16 12:29:45', 6, 8, 7),
(38, '2023-06-16 12:30:23', 6, 8, 7),
(39, '2023-06-16 12:41:24', 6, 7, 8),
(40, '2023-06-16 12:42:22', 6, 7, 8),
(41, '2023-06-16 16:31:45', 6, 8, 7),
(42, '2023-06-16 16:31:59', 6, 7, 8),
(43, '2023-06-16 16:32:54', 6, 10, 8);

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `players`
--

CREATE TABLE `players` (
  `id` int(11) NOT NULL,
  `username` varchar(20) NOT NULL,
  `password` varchar(255) NOT NULL,
  `email` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Gegevens worden geëxporteerd voor tabel `players`
--

INSERT INTO `players` (`id`, `username`, `password`, `email`) VALUES
(1, 'player1', 'password', 'player1@email.com'),
(2, 'player2', 'passworddd', 'player2@email.com'),
(3, 'shooterperson', '1234', 'shooterPerson@email.com'),
(4, 'hackerman', 'hackerman', 'hacker@man.com'),
(5, 'abi', 'sabi', 'abi@sabi.com'),
(6, 'player6', 'cool', 'player6@game.com'),
(7, 'qwer', '$2y$10$aNsFQsFGTCt1TeOEUce1BOLtc4eyvutr/v1TY4zXdbtPYw7mfaf4G', 'qwer@qwer.qwer'),
(8, '1234', '$2y$10$XUQUoHXt8.3LCP.ZjTa16OJISZa/CdOWg4KUXqxcbfKdI/ie2pt.S', '1234@1234.1234'),
(9, '1235', '$2y$10$fESI6W9ZoGhJf6VXC/jTyeCAXGXNhQdPgDhquhyIT/Mzh874o6Q8e', '1234@1234.1235'),
(10, 'maxym', '$2y$10$DiHX6ni2bLGAuGK10Ed9yuadf7b3Gk.k2.FqdHYch/Lgjc4lEfI7W', 'maxym@maxym.com');

--
-- Indexen voor geëxporteerde tabellen
--

--
-- Indexen voor tabel `games`
--
ALTER TABLE `games`
  ADD PRIMARY KEY (`id`);

--
-- Indexen voor tabel `gdv_server`
--
ALTER TABLE `gdv_server`
  ADD PRIMARY KEY (`id`);

--
-- Indexen voor tabel `played_games`
--
ALTER TABLE `played_games`
  ADD PRIMARY KEY (`id`);

--
-- Indexen voor tabel `players`
--
ALTER TABLE `players`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT voor geëxporteerde tabellen
--

--
-- AUTO_INCREMENT voor een tabel `games`
--
ALTER TABLE `games`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT voor een tabel `gdv_server`
--
ALTER TABLE `gdv_server`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT voor een tabel `played_games`
--
ALTER TABLE `played_games`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=44;

--
-- AUTO_INCREMENT voor een tabel `players`
--
ALTER TABLE `players`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
