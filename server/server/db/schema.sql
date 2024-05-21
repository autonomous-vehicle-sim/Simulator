DROP TABLE IF EXISTS `frame`;
DROP TABLE IF EXISTS `vehicle`;
DROP TABLE IF EXISTS `map`;

CREATE TABLE `map` (
    id INT AUTO_INCREMENT PRIMARY KEY,
    seed INT NOT NULL
);

CREATE TABLE `vehicle` (
    map_id INT NOT NULL,
    vehicle_id INT NOT NULL,
    PRIMARY KEY (map_id, vehicle_id),
    FOREIGN KEY (map_id) REFERENCES map(id) ON DELETE CASCADE
);

CREATE TABLE `frame` (
    id INT AUTO_INCREMENT PRIMARY KEY,
    map_id INT NOT NULL,
    vehicle_id INT NOT NULL,
    FOREIGN KEY (map_id, vehicle_id) REFERENCES vehicle(map_id, vehicle_id) ON DELETE CASCADE,
    path_camera1 VARCHAR(255) NOT NULL,
    path_camera2 VARCHAR(255) NOT NULL,
    path_camera3 VARCHAR(255) NOT NULL
);
