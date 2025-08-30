/*
 Navicat Premium Dump SQL

 Source Server         : 124.220.33.219_A2iH!Rz8ZWnt3!p
 Source Server Type    : MySQL
 Source Server Version : 80405 (8.4.5)
 Source Host           : 124.220.33.219:3306
 Source Schema         : findx_eleadminplus

 Target Server Type    : MySQL
 Target Server Version : 80405 (8.4.5)
 File Encoding         : 65001

 Date: 30/08/2025 18:04:39
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for config_apps
-- ----------------------------
DROP TABLE IF EXISTS `config_apps`;
CREATE TABLE `config_apps` (
  `Id` bigint NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `AppId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Secret` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `CreatorId` bigint DEFAULT NULL,
  `CreatedTime` datetime(3) DEFAULT NULL,
  `LastUpdaterId` bigint DEFAULT NULL,
  `LastUpdatedTime` datetime(3) DEFAULT NULL,
  `DeletionTime` datetime(3) DEFAULT NULL,
  `IsDeleted` bit(1) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Records of config_apps
-- ----------------------------
BEGIN;
INSERT INTO `config_apps` (`Id`, `Name`, `AppId`, `Secret`, `CreatorId`, `CreatedTime`, `LastUpdaterId`, `LastUpdatedTime`, `DeletionTime`, `IsDeleted`) VALUES (7441692361016355930, '测试', 'findx', 'test', NULL, '2023-03-16 10:04:18.000', NULL, NULL, NULL, b'0');
COMMIT;

-- ----------------------------
-- Table structure for config_info
-- ----------------------------
DROP TABLE IF EXISTS `config_info`;
CREATE TABLE `config_info` (
  `Id` bigint NOT NULL,
  `AppId` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `DataId` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `DataType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  `Environment` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Version` bigint NOT NULL,
  `Md5` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Comments` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `CreatorId` bigint DEFAULT NULL,
  `CreatedTime` datetime(3) DEFAULT NULL,
  `LastUpdaterId` bigint DEFAULT NULL,
  `LastUpdatedTime` datetime(3) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Records of config_info
-- ----------------------------
BEGIN;
INSERT INTO `config_info` (`Id`, `AppId`, `DataId`, `DataType`, `Content`, `Environment`, `Version`, `Md5`, `Comments`, `CreatorId`, `CreatedTime`, `LastUpdaterId`, `LastUpdatedTime`) VALUES (7441692361016355931, 'findx', 'test', 'Text', 'string8', 'dev', 1, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `config_info` (`Id`, `AppId`, `DataId`, `DataType`, `Content`, `Environment`, `Version`, `Md5`, `Comments`, `CreatorId`, `CreatedTime`, `LastUpdaterId`, `LastUpdatedTime`) VALUES (7441692361016355932, 'findx', 'freeSql', 'Json', '{\"Findx\":{\"FreeSql\":{\"Enabled\":true,\"Primary\":\"system\",\"Strict\":true,\"PrintSql\":false,\"UseAutoSyncStructure\":true,\"OutageDetection\":true,\"OutageDetectionInterval\":1,\"SoftDeletable\":true,\"MultiTenant\":true,\"CheckInsert\":true,\"CheckUpdate\":true,\"DataSource\":{\"system\":{\"ConnectionString\":\"Data Source=106.54.160.19;Port=3306;User ID=root;Password=tBXV2gGWyp8puy41;Initial Catalog=findx_eleadmin;Charset=utf8mb4;SslMode=none;Min pool size=1\",\"DbType\":\"MySql\",\"DataSourceSharing\":[\"config\"]}}}}}', 'dev', 1, NULL, NULL, NULL, NULL, NULL, NULL);
COMMIT;

-- ----------------------------
-- Table structure for config_info_history
-- ----------------------------
DROP TABLE IF EXISTS `config_info_history`;
CREATE TABLE `config_info_history` (
  `Id` bigint NOT NULL,
  `AppId` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `DataId` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `DataType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
  `Environment` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Version` bigint NOT NULL,
  `Comments` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `CreatorId` bigint DEFAULT NULL,
  `CreatedTime` datetime(3) DEFAULT NULL,
  `LastUpdaterId` bigint DEFAULT NULL,
  `LastUpdatedTime` datetime(3) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Records of config_info_history
-- ----------------------------
BEGIN;
COMMIT;

-- ----------------------------
-- Table structure for sys_dict_data
-- ----------------------------
DROP TABLE IF EXISTS `sys_dict_data`;
CREATE TABLE `sys_dict_data` (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '字典项id',
  `typeId` bigint NOT NULL COMMENT '字典id',
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '字典项名称',
  `value` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '字典项标识',
  `sort` int NOT NULL DEFAULT '1' COMMENT '排序号',
  `comments` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '备注',
  `creatorId` bigint DEFAULT NULL COMMENT '创建人',
  `createdTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `lastUpdaterId` bigint DEFAULT NULL COMMENT '更新人',
  `lastUpdatedTime` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `isDeleted` tinyint NOT NULL DEFAULT '0' COMMENT '是否删除',
  `deletionTime` datetime DEFAULT NULL COMMENT '删除时间',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `type_id` (`typeId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='字典项';

-- ----------------------------
-- Records of sys_dict_data
-- ----------------------------
BEGIN;
INSERT INTO `sys_dict_data` (`id`, `typeId`, `name`, `value`, `sort`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (1, 1, '男', '1', 1, NULL, NULL, '2024-06-19 17:26:01', 40, '2024-06-19 17:26:02', 0, NULL);
INSERT INTO `sys_dict_data` (`id`, `typeId`, `name`, `value`, `sort`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (2, 1, '女', '2', 2, NULL, NULL, '2024-06-19 17:25:59', 40, '2024-06-19 17:26:00', 0, NULL);
INSERT INTO `sys_dict_data` (`id`, `typeId`, `name`, `value`, `sort`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3, 2, '公司', '1', 1, NULL, NULL, '2023-11-13 14:37:10', NULL, NULL, 0, NULL);
INSERT INTO `sys_dict_data` (`id`, `typeId`, `name`, `value`, `sort`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (4, 2, '子公司', '2', 2, NULL, NULL, '2023-11-13 14:37:10', NULL, NULL, 0, NULL);
INSERT INTO `sys_dict_data` (`id`, `typeId`, `name`, `value`, `sort`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (5, 2, '部门', '3', 3, NULL, NULL, '2023-11-13 14:37:10', NULL, NULL, 0, NULL);
INSERT INTO `sys_dict_data` (`id`, `typeId`, `name`, `value`, `sort`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (6, 2, '小组', '4', 4, NULL, NULL, '2023-11-13 14:37:10', NULL, NULL, 0, NULL);
COMMIT;

-- ----------------------------
-- Table structure for sys_dict_type
-- ----------------------------
DROP TABLE IF EXISTS `sys_dict_type`;
CREATE TABLE `sys_dict_type` (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '字典id',
  `code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '字典标识',
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '字典名称',
  `sort` int NOT NULL DEFAULT '1' COMMENT '排序号',
  `comments` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '备注',
  `creatorId` bigint DEFAULT NULL COMMENT '创建人',
  `createdTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `lastUpdaterId` bigint DEFAULT NULL COMMENT '更新人',
  `lastUpdatedTime` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `isDeleted` tinyint NOT NULL DEFAULT '0' COMMENT '是否删除',
  `deletionTime` datetime DEFAULT NULL COMMENT '删除时间',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='字典';

-- ----------------------------
-- Records of sys_dict_type
-- ----------------------------
BEGIN;
INSERT INTO `sys_dict_type` (`id`, `code`, `name`, `sort`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (1, 'sex', '性别', 1, NULL, NULL, '2023-11-13 14:39:05', NULL, NULL, 0, NULL);
INSERT INTO `sys_dict_type` (`id`, `code`, `name`, `sort`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (2, 'org_type', '机构类型', 2, NULL, NULL, '2023-11-13 15:09:58', NULL, '2023-11-13 15:09:58', 0, NULL);
COMMIT;

-- ----------------------------
-- Table structure for sys_login_record
-- ----------------------------
DROP TABLE IF EXISTS `sys_login_record`;
CREATE TABLE `sys_login_record` (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '主键',
  `userName` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '用户账号',
  `nickname` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '用户账号',
  `os` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '操作系统',
  `device` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '设备名',
  `browser` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '浏览器类型',
  `ip` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'ip地址',
  `loginType` int NOT NULL COMMENT '操作类型, 0登录成功, 1登录失败, 2退出登录, 3续签token',
  `comments` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '备注',
  `tenantId` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '租户id',
  `createdTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `tenant_id` (`tenantId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4919704262519156826 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='登录日志';

-- ----------------------------
-- Records of sys_login_record
-- ----------------------------
BEGIN;
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (1, 'admin', '管理员', 'Mac', 'OSX', 'MSEdge', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2023-11-13 16:30:23');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (2, 'admin', '管理员', 'Mac', 'OSX', 'MSEdge', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2023-11-13 17:14:26');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (3, 'admin', '管理员', 'Mac', 'OSX', 'MSEdge', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2023-11-13 17:17:23');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (4, 'admin', '管理员', 'Mac', 'OSX', 'MSEdge', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2023-11-16 17:00:13');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (5, 'admin', '管理员', 'Mac', 'OSX', 'MSEdge', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2023-11-16 17:00:21');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (6, 'admin', '管理员', 'Mac', 'OSX', 'MSEdge', '183.167.208.241', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2023-11-24 15:19:10');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (7, 'admin', '管理员', 'Mac', 'OSX', 'MSEdge', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2023-12-01 09:23:24');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (8, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', '2023-12-18 15:59:39');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (9, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 1, '账号密码错误,剩余重试次数3次', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', '2023-12-18 16:00:43');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (10, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 0, '登录成功', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', '2023-12-18 16:02:58');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (11, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 0, '登录成功', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', '2023-12-18 16:07:41');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (12, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 0, '登录成功', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', '2023-12-18 16:12:06');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (13, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 0, '登录成功', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', '2023-12-18 16:29:29');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (14, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 0, '登录成功', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', '2023-12-18 16:32:44');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (15, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 0, '登录成功', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', '2023-12-20 14:47:31');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280920187308773377, 'admin', '管理员', 'Mac', 'OSX', 'MSEdge', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2023-12-21 15:35:57');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280920187308773378, 'admin', '管理员', 'Mac', 'OSX', 'MSEdge', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-01-03 14:39:06');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971773338378241, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-15 10:00:03');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971773338378242, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数3次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-15 10:00:13');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971779886555137, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-15 10:26:41');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971779886555138, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-15 10:42:02');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971779886555139, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-15 10:42:09');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971779886555257, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-19 15:24:45');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971779886555258, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数3次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-19 15:24:53');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971779886555259, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-19 15:24:59');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971779886555260, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-19 16:33:55');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280971779886555261, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-19 16:34:04');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280975688968003672, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-28 15:19:23');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280975688968003673, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-05-28 15:19:31');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280983907947888728, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-06-18 16:57:07');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280983907947888729, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-06-18 16:57:12');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280983907947888730, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-06-18 16:57:46');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280983907947888731, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-06-18 16:57:50');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280986979776458840, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-04 14:20:33');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280986979776458841, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-04 14:20:39');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280989928060653656, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-05 17:12:43');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280989928060653657, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-05 17:12:47');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280989928060653658, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-11 15:04:01');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280989928060653659, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-11 15:04:05');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280994413235736645, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '60.172.145.64', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-18 15:35:16');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (280994413235736646, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '60.172.145.64', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-18 15:35:21');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (4289193584830677080, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '127.0.0.1', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-05 16:24:42');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (4289193584830677081, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '127.0.0.1', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-05 16:24:55');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (4289195103430164568, 'admin', '管理员', 'iPhone', 'iPhone', 'Safari', '127.0.0.1', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-07-09 23:43:09');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (4289209506518569033, 'admin', '管理员', 'Mac', 'OSX', 'Chrome', '127.0.0.1', 0, '登录成功', '3a0e9b79-24bd-182a-522d-09067cf926ae', '2024-08-19 16:11:56');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (4919704262519156824, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 1, '账号密码错误,剩余重试次数4次', '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', '2024-07-24 16:55:23');
INSERT INTO `sys_login_record` (`id`, `userName`, `nickname`, `os`, `device`, `browser`, `ip`, `loginType`, `comments`, `tenantId`, `createdTime`) VALUES (4919704262519156825, 'admin', '管理员', 'Windows', 'Windows 10 or Windows Server 2016', 'MSEdge', '127.0.0.1', 0, '登录成功', '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', '2024-07-24 16:56:12');
COMMIT;

-- ----------------------------
-- Table structure for sys_menu
-- ----------------------------
DROP TABLE IF EXISTS `sys_menu`;
CREATE TABLE `sys_menu` (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '菜单id',
  `parentId` bigint NOT NULL DEFAULT '0' COMMENT '上级id, 0是顶级',
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '菜单名称',
  `path` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '菜单路由地址',
  `component` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '菜单组件地址, 目录可为空',
  `menuType` int DEFAULT '0' COMMENT '类型, 0菜单, 1按钮',
  `sort` int NOT NULL DEFAULT '1' COMMENT '排序号',
  `authority` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '权限标识',
  `icon` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '菜单图标',
  `hide` int NOT NULL DEFAULT '0' COMMENT '是否隐藏, 0否, 1是(仅注册路由不显示在左侧菜单)',
  `meta` varchar(800) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '其它路由元信息',
  `tenantId` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '租户id',
  `creatorId` bigint DEFAULT NULL COMMENT '创建人',
  `createdTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `lastUpdaterId` bigint DEFAULT NULL COMMENT '更新人',
  `lastUpdatedTime` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `isDeleted` tinyint NOT NULL DEFAULT '0' COMMENT '是否删除',
  `deletionTime` datetime DEFAULT NULL COMMENT '删除时间',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=280971779886555264 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='菜单';

-- ----------------------------
-- Records of sys_menu
-- ----------------------------
BEGIN;
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (1, 0, '系统管理', '/system', NULL, 0, 1, NULL, 'el-icon-setting', 0, '{\"badge\": \"New\", \"badgeColor\": \"warning\"}', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (2, 1, '用户管理', '/system/user', '/system/user', 0, 1, NULL, 'el-icon-_user-group', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3, 2, '查询用户', NULL, NULL, 1, 1, 'sys:user:list', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (4, 2, '添加用户', NULL, NULL, 1, 2, 'sys:user:save', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (5, 2, '修改用户', NULL, NULL, 1, 3, 'sys:user:update', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (6, 2, '删除用户', NULL, NULL, 1, 4, 'sys:user:remove', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (7, 1, '角色管理', '/system/role', '/system/role', 0, 2, NULL, 'el-icon-postcard', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (8, 7, '查询角色', NULL, NULL, 1, 1, 'sys:role:list', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (9, 7, '添加角色', NULL, NULL, 1, 2, 'sys:role:save', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (10, 7, '修改角色', NULL, NULL, 1, 3, 'sys:role:update', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:48', NULL, '2023-11-13 15:48:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (11, 7, '删除角色', NULL, NULL, 1, 4, 'sys:role:remove', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (12, 1, '菜单管理', '/system/menu', '/system/menu', 0, 3, NULL, 'el-icon-s-operation', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (13, 12, '查询菜单', NULL, NULL, 1, 1, 'sys:menu:list', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (14, 12, '添加菜单', NULL, NULL, 1, 2, 'sys:menu:save', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (15, 12, '修改菜单', NULL, NULL, 1, 3, 'sys:menu:update', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (16, 12, '删除菜单', NULL, NULL, 1, 4, 'sys:menu:remove', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (17, 1, '机构管理', '/system/organization', '/system/organization', 0, 4, NULL, 'el-icon-office-building', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (18, 17, '查询机构', NULL, NULL, 1, 1, 'sys:org:list', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (19, 17, '添加机构', NULL, NULL, 1, 2, 'sys:org:save', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (20, 17, '修改机构', NULL, NULL, 1, 3, 'sys:org:update', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (21, 17, '删除机构', NULL, NULL, 1, 4, 'sys:org:remove', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (22, 1, '字典管理', '/system/dictionary', '/system/dictionary', 0, 5, NULL, 'el-icon-notebook-2', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (23, 22, '查询字典', NULL, NULL, 1, 1, 'sys:dict:list', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (24, 22, '添加字典', NULL, NULL, 1, 2, 'sys:dict:save', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (25, 22, '修改字典', NULL, NULL, 1, 3, 'sys:dict:update', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (26, 22, '删除字典', NULL, NULL, 1, 4, 'sys:dict:remove', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (27, 1, '登录日志', '/system/login-record', '/system/login-record', 0, 7, 'sys:login-record:list', 'el-icon-date', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (28, 1, '操作日志', '/system/operation-record', '/system/operation-record', 0, 8, 'sys:operation-record:list', 'el-icon-_retrieve', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (29, 1, '文件管理', '/system/file', '/system/file', 0, 6, NULL, 'el-icon-folder', 1, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (30, 29, '上传文件', NULL, NULL, 1, 1, 'sys:file:upload', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (31, 29, '删除文件', NULL, NULL, 1, 2, 'sys:file:remove', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (32, 29, '查看记录', NULL, NULL, 1, 3, 'sys:file:list', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:49', NULL, '2023-11-13 15:48:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (33, 2, '用户详情', '/system/user/details', '/system/user/details', 0, 5, NULL, 'el-icon-user', 1, '{\"active\": \"/system/user\"}', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (34, 1, '修改个人密码', NULL, NULL, 1, 9, 'sys:auth:password', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (35, 1, '修改个人资料', NULL, NULL, 1, 10, 'sys:auth:user', NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (36, 0, 'Dashboard', '/dashboard', NULL, 0, 0, NULL, 'el-icon-house', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (37, 36, '工作台', '/dashboard/workplace', '/dashboard/workplace', 0, 1, NULL, 'el-icon-monitor', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (38, 36, '分析页', '/dashboard/analysis', '/dashboard/analysis', 0, 2, NULL, 'el-icon-data-analysis', 0, '{\"badge\": 1}', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (39, 36, '监控页', '/dashboard/monitor', '/dashboard/monitor', 0, 3, NULL, 'el-icon-odometer', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (40, 0, '表单页面', '/form', NULL, 0, 2, NULL, 'el-icon-tickets', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (41, 40, '基础表单', '/form/basic', '/form/basic', 0, 1, NULL, 'el-icon-_feedback', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (42, 40, '复杂表单', '/form/advanced', '/form/advanced', 0, 2, NULL, 'el-icon-_visa', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (43, 40, '分步表单', '/form/step', '/form/step', 0, 3, NULL, 'el-icon-c-scale-to-original', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (44, 0, '列表页面', '/list', NULL, 0, 3, NULL, 'el-icon-_table', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (45, 44, '基础列表', '/list/basic', '/list/basic', 0, 1, NULL, 'el-icon-document', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (46, 44, '复杂列表', '/list/advanced', '/list/advanced', 0, 2, NULL, 'el-icon-_cols', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (47, 44, '卡片列表', '/list/card', '/list/card', 0, 3, NULL, 'el-icon-_menu', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (48, 47, '项目列表', '/list/card/project', '/list/card/project', 0, 1, NULL, 'el-icon-_menu', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (49, 47, '应用列表', '/list/card/application', '/list/card/application', 0, 2, NULL, 'el-icon-_menu', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (50, 47, '文章列表', '/list/card/article', '/list/card/article', 0, 3, NULL, 'el-icon-_menu', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (51, 45, '添加用户', '/list/basic/add', '/list/basic/add', 0, 1, NULL, 'el-icon-user', 1, '{\"active\": \"/list/basic\"}', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (52, 45, '修改用户', '/list/basic/edit', '/list/basic/edit', 0, 2, NULL, 'el-icon-user', 1, '{\"active\": \"/list/basic\"}', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (53, 45, '用户详情', '/list/basic/details/:id', '/list/basic/details', 0, 3, NULL, 'el-icon-user', 1, '{\"active\": \"/list/basic\"}', '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (54, 0, '结果页面', '/result', NULL, 0, 4, NULL, 'el-icon-circle-check', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (55, 54, '成功页', '/result/success', '/result/success', 0, 1, NULL, 'el-icon-circle-check', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (56, 54, '失败页', '/result/fail', '/result/fail', 0, 2, NULL, 'el-icon-circle-close', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:50', NULL, '2023-11-13 15:48:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (57, 0, '异常页面', '/exception', NULL, 0, 5, NULL, 'el-icon-document-delete', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (58, 57, '403', '/exception/403', '/exception/403', 0, 1, NULL, 'el-icon-document-checked', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (59, 57, '404', '/exception/404', '/exception/404', 0, 2, NULL, 'el-icon-document-remove', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (60, 57, '500', '/exception/500', '/exception/500', 0, 3, NULL, 'el-icon-document-delete', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (61, 0, '个人中心', '/user', NULL, 0, 6, NULL, 'el-icon-set-up', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (62, 61, '我的资料', '/user/profile', '/user/profile', 0, 1, NULL, 'el-icon-user', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (63, 61, '我的消息', '/user/message', '/user/message', 0, 2, NULL, 'el-icon-chat-dot-round', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (64, 0, '扩展组件', '/extension', NULL, 0, 7, NULL, 'el-icon-open', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (65, 64, '图标扩展', '/extension/icon', '/extension/icon', 0, 1, NULL, 'el-icon-_heart', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (66, 64, '标签组件', '/extension/tag', '/extension/tag', 0, 2, NULL, 'el-icon-price-tag', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (67, 64, '弹窗扩展', '/extension/dialog', '/extension/dialog', 0, 3, NULL, 'el-icon-copy-document', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (68, 64, '文件列表', '/extension/file', '/extension/file', 0, 4, NULL, 'el-icon-folder', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (69, 64, '图片上传', '/extension/upload', '/extension/upload', 0, 5, NULL, 'el-icon-picture-outline', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (70, 64, '拖拽排序', '/extension/dragsort', '/extension/dragsort', 0, 6, NULL, 'el-icon-rank', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (71, 64, '消息提示', '/extension/message', '/extension/message', 0, 7, NULL, 'el-icon-chat-line-square', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (72, 64, '城市选择', '/extension/regions', '/extension/regions', 0, 8, NULL, 'el-icon-office-building', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (73, 64, '打印插件', '/extension/printer', '/extension/printer', 0, 9, NULL, 'el-icon-printer', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (74, 64, 'excel插件', '/extension/excel', '/extension/excel', 0, 10, NULL, 'el-icon-_table', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (75, 64, '滚动数字', '/extension/count-up', '/extension/count-up', 0, 11, NULL, 'el-icon-more', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:51', NULL, '2023-11-13 15:48:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (76, 64, '空状态', '/extension/empty', '/extension/empty', 0, 12, NULL, 'el-icon-receiving', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (77, 64, '步骤条', '/extension/steps', '/extension/steps', 0, 13, NULL, 'el-icon-_timeline', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (78, 64, '菜单导航', '/extension/menu', '/extension/menu', 0, 14, NULL, 'el-icon-s-operation', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (79, 64, '树形下拉', '/extension/tree-select', '/extension/tree-select', 0, 15, NULL, 'el-icon-_condition', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (80, 64, '表格下拉', '/extension/table-select', '/extension/table-select', 0, 16, NULL, 'el-icon-_table', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (81, 64, '分割布局', '/extension/split-layout', '/extension/split-layout', 0, 17, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (82, 64, '视频播放', '/extension/player', '/extension/player', 0, 18, NULL, 'el-icon-_video', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (83, 64, '地图组件', '/extension/map', '/extension/map', 0, 19, NULL, 'el-icon-map-location', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (84, 64, '二维码', '/extension/qr-code', '/extension/qr-code', 0, 20, NULL, 'el-icon-_qrcode', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (85, 64, '条形码', '/extension/bar-code', '/extension/bar-code', 0, 21, NULL, 'el-icon-_barcode', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (86, 64, '富文本框', '/extension/editor', '/extension/editor', 0, 22, NULL, 'el-icon-_font-family', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (87, 64, 'markdown', '/extension/markdown', '/extension/markdown', 0, 23, NULL, 'el-icon-_feedback', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (88, 64, '仪表盘', '/extension/dashboard', '/extension/dashboard', 0, 24, NULL, 'el-icon-_menu', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (89, 64, '引导组件', '/extension/tour', '/extension/tour', 0, 25, NULL, 'el-icon-_menu', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (90, 0, '常用实例', '/example', NULL, 0, 8, NULL, 'el-icon-_integral', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (91, 90, '表格实例', '/example/table', '/example/table', 0, 1, NULL, 'el-icon-_table', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (92, 90, '菜单徽章', '/example/menu-badge', '/example/menu-badge', 0, 2, NULL, 'el-icon-_menu', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (93, 90, '内嵌页面', '/example/eleadmin', 'https://www.eleadmin.com', 0, 3, NULL, 'el-icon-connection', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (94, 90, '内嵌文档', '/example/eleadmin-doc', 'https://eleadmin.com/doc/eleadmin/', 0, 4, NULL, 'el-icon-connection', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (95, 90, '批量选择', '/example/choose', '/example/choose', 0, 5, NULL, 'el-icon-finished', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:52', NULL, '2023-11-13 15:48:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (96, 90, '案卷调整', '/example/document', '/example/document', 0, 6, NULL, 'el-icon-files', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:53', NULL, '2023-11-13 15:48:53', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (97, 64, '水印组件', '/extension/watermark', '/extension/watermark', 0, 26, NULL, 'el-icon-_menu', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:53', NULL, '2023-11-13 15:48:53', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (98, 64, '分割面板', '/extension/split', '/extension/split', 0, 27, NULL, 'el-icon-_menu', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:53', NULL, '2023-11-13 15:48:53', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (99, 0, '获取授权', 'https://eleadmin.com/goods/8', NULL, 0, 9, NULL, 'el-icon-_prerogative', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:48:53', NULL, '2023-11-13 15:48:53', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (101, 0, '系统管理', '/system', NULL, 0, 1, NULL, 'setting-outlined', 0, '{\"badge\": \"New\", \"badgeColor\": \"#faad14\"}', '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:04', NULL, '2023-11-13 15:49:04', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (102, 101, '用户管理', '/system/user', '/system/user', 0, 1, NULL, 'team-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:04', NULL, '2023-11-13 15:49:04', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (103, 102, '查询用户', NULL, NULL, 1, 1, 'sys:user:list', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:04', NULL, '2023-11-13 15:49:04', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (104, 102, '添加用户', NULL, NULL, 1, 2, 'sys:user:save', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:04', NULL, '2023-11-13 15:49:04', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (105, 102, '修改用户', NULL, NULL, 1, 3, 'sys:user:update', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:04', NULL, '2023-11-13 15:49:04', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (106, 102, '删除用户', NULL, NULL, 1, 4, 'sys:user:remove', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:04', NULL, '2023-11-13 15:49:04', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (107, 101, '角色管理', '/system/role', '/system/role', 0, 2, NULL, 'idcard-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:04', NULL, '2023-11-13 15:49:04', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (108, 107, '查询角色', NULL, NULL, 1, 1, 'sys:role:list', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:04', NULL, '2023-11-13 15:49:04', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (109, 107, '添加角色', NULL, NULL, 1, 2, 'sys:role:save', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:04', NULL, '2023-11-13 15:49:04', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (110, 107, '修改角色', NULL, NULL, 1, 3, 'sys:role:update', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (111, 107, '删除角色', NULL, NULL, 1, 4, 'sys:role:remove', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (112, 101, '菜单管理', '/system/menu', '/system/menu', 0, 3, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (113, 112, '查询菜单', NULL, NULL, 1, 1, 'sys:menu:list', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (114, 112, '添加菜单', NULL, NULL, 1, 2, 'sys:menu:save', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (115, 112, '修改菜单', NULL, NULL, 1, 3, 'sys:menu:update', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (116, 112, '删除菜单', NULL, NULL, 1, 4, 'sys:menu:remove', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (117, 101, '机构管理', '/system/organization', '/system/organization', 0, 5, NULL, 'bank-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (118, 117, '查询机构', NULL, NULL, 1, 1, 'sys:org:list', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (119, 117, '添加机构', NULL, NULL, 1, 2, 'sys:org:save', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (120, 117, '修改机构', NULL, NULL, 1, 3, 'sys:org:update', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (121, 117, '删除机构', NULL, NULL, 1, 4, 'sys:org:remove', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (122, 101, '字典管理', '/system/dictionary', '/system/dictionary', 0, 4, NULL, 'profile-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:05', NULL, '2023-11-13 15:49:05', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (123, 122, '查询字典', NULL, NULL, 1, 1, 'sys:dict:list', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (124, 122, '添加字典', NULL, NULL, 1, 2, 'sys:dict:save', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (125, 122, '修改字典', NULL, NULL, 1, 3, 'sys:dict:update', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (126, 122, '删除字典', NULL, NULL, 1, 4, 'sys:dict:remove', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (127, 101, '登录日志', '/system/login-record', '/system/login-record', 0, 7, 'sys:login-record:list', 'calendar-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (128, 101, '操作日志', '/system/operation-record', '/system/operation-record', 0, 8, 'sys:operation-record:list', 'file-search-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (129, 101, '文件管理', '/system/file', '/system/file', 0, 6, NULL, 'folder-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (130, 129, '上传文件', NULL, NULL, 1, 1, 'sys:file:upload', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (131, 129, '删除文件', NULL, NULL, 1, 2, 'sys:file:remove', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (132, 129, '查看记录', NULL, NULL, 1, 3, 'sys:file:list', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (133, 102, '用户详情', '/system/user/details', '/system/user/details', 0, 5, NULL, 'team-outlined', 1, '{\"active\": \"/system/user\"}', '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (134, 101, '修改个人密码', NULL, NULL, 1, 10, 'sys:auth:password', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:06', NULL, '2023-11-13 15:49:06', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (135, 101, '修改个人资料', NULL, NULL, 1, 11, 'sys:auth:user', NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (136, 0, 'Dashboard', '/dashboard', NULL, 0, 0, NULL, 'home-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (137, 136, '工作台', '/dashboard/workplace', '/dashboard/workplace', 0, 1, NULL, 'desktop-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (138, 136, '分析页', '/dashboard/analysis', '/dashboard/analysis', 0, 2, NULL, 'bar-chart-outlined', 0, '{\"badge\": 1}', '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (139, 136, '监控页', '/dashboard/monitor', '/dashboard/monitor', 0, 3, NULL, 'dashboard-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (140, 0, '表单页面', '/form', NULL, 0, 2, NULL, 'file-text-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (141, 140, '基础表单', '/form/basic', '/form/basic', 0, 1, NULL, 'file-text-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (142, 140, '复杂表单', '/form/advanced', '/form/advanced', 0, 2, NULL, 'audit-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (143, 140, '分步表单', '/form/step', '/form/step', 0, 3, NULL, 'one-to-one-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (144, 0, '列表页面', '/list', NULL, 0, 3, NULL, 'table-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (145, 144, '基础列表', '/list/basic', '/list/basic', 0, 1, NULL, 'table-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (146, 144, '复杂列表', '/list/advanced', '/list/advanced', 0, 2, NULL, 'profile-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (147, 144, '卡片列表', '/list/card', '/list/card', 0, 3, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (148, 147, '项目列表', '/list/card/project', '/list/card/project', 0, 1, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (149, 147, '应用列表', '/list/card/application', '/list/card/application', 0, 2, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (150, 147, '文章列表', '/list/card/article', '/list/card/article', 0, 3, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (151, 145, '添加用户', '/list/basic/add', '/list/basic/add', 0, 4, NULL, 'team-outlined', 1, '{\"active\": \"/list/basic\"}', '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (152, 145, '修改用户', '/list/basic/edit', '/list/basic/edit', 0, 4, NULL, 'team-outlined', 1, '{\"active\": \"/list/basic\"}', '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (153, 145, '用户详情', '/list/basic/details/:id', '/list/basic/details', 0, 4, NULL, 'team-outlined', 1, '{\"active\": \"/list/basic\"}', '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (154, 0, '结果页面', '/result', NULL, 0, 4, NULL, 'check-circle-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:07', NULL, '2023-11-13 15:49:07', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (155, 154, '成功页', '/result/success', '/result/success', 0, 1, NULL, 'check-circle-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (156, 154, '失败页', '/result/fail', '/result/fail', 0, 2, NULL, 'close-circle-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (157, 0, '异常页面', '/exception', NULL, 0, 5, NULL, 'warning-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (158, 157, '403', '/exception/403', '/exception/403', 0, 1, NULL, 'exclamation-circle-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (159, 157, '404', '/exception/404', '/exception/404', 0, 2, NULL, 'question-circle-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (160, 157, '500', '/exception/500', '/exception/500', 0, 3, NULL, 'close-circle-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (161, 0, '个人中心', '/user', NULL, 0, 6, NULL, 'control-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (162, 161, '我的资料', '/user/profile', '/user/profile', 0, 1, NULL, 'user-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (163, 161, '我的消息', '/user/message', '/user/message', 0, 2, NULL, 'sound-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (164, 0, '扩展组件', '/extension', NULL, 0, 7, NULL, 'appstore-add-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (165, 164, '标签组件', '/extension/tag', '/extension/tag', 0, 1, NULL, 'tag-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (166, 164, '弹窗扩展', '/extension/dialog', '/extension/dialog', 0, 2, NULL, 'block-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:08', NULL, '2023-11-13 15:49:08', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (167, 164, '文件列表', '/extension/file', '/extension/file', 0, 3, NULL, 'folder-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (168, 164, '图片上传', '/extension/upload', '/extension/upload', 0, 4, NULL, 'picture-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (169, 164, '拖拽排序', '/extension/dragsort', '/extension/dragsort', 0, 5, NULL, 'drag-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (170, 164, '颜色选择', '/extension/color-picker', '/extension/color-picker', 0, 6, NULL, 'bg-colors-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (171, 164, '城市选择', '/extension/regions', '/extension/regions', 0, 7, NULL, 'apartment-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (172, 164, '打印插件', '/extension/printer', '/extension/printer', 0, 8, NULL, 'printer-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (173, 164, 'excel插件', '/extension/excel', '/extension/excel', 0, 9, NULL, 'table-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (174, 164, '滚动数字', '/extension/count-up', '/extension/count-up', 0, 10, NULL, 'ellipsis-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (175, 164, '无限滚动', '/extension/infinite-scroll', '/extension/infinite-scroll', 0, 11, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (176, 164, '表格下拉', '/extension/table-select', '/extension/table-select', 0, 12, NULL, 'table-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (177, 164, '分割布局', '/extension/split-layout', '/extension/split-layout', 0, 13, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (178, 164, '视频播放', '/extension/player', '/extension/player', 0, 14, NULL, 'youtube-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (179, 164, '地图组件', '/extension/map', '/extension/map', 0, 15, NULL, 'environment-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (180, 164, '二维码', '/extension/qr-code', '/extension/qr-code', 0, 16, NULL, 'qrcode-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (181, 164, '条形码', '/extension/bar-code', '/extension/bar-code', 0, 17, NULL, 'barcode-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (182, 164, '富文本框', '/extension/editor', '/extension/editor', 0, 18, NULL, 'font-size-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (183, 164, 'markdown', '/extension/markdown', '/extension/markdown', 0, 19, NULL, 'picLeft-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (184, 164, '仪表盘', '/extension/dashboard', '/extension/dashboard', 0, 20, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:09', NULL, '2023-11-13 15:49:09', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (185, 164, '引导组件', '/extension/tour', '/extension/tour', 0, 21, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (186, 164, '水印组件', '/extension/watermark', '/extension/watermark', 0, 22, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (187, 164, '分割面板', '/extension/split', '/extension/split', 0, 23, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (188, 0, '常用实例', '/example', NULL, 0, 8, NULL, 'compass-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (189, 188, '表格实例', '/example/table', '/example/table', 0, 1, NULL, 'table-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (190, 188, '菜单徽章', '/example/menu-badge', '/example/menu-badge', 0, 5, NULL, 'appstore-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (191, 188, '内嵌页面', '/example/eleadmin', 'https://www.eleadmin.com', 0, 2, NULL, 'link-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (192, 188, '内嵌文档', '/example/eleadmin-doc', 'https://www.eleadmin.com/doc/eleadminpro/', 0, 3, NULL, 'link-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (193, 188, '批量选择', '/example/choose', '/example/choose', 0, 4, NULL, 'check-square-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (194, 188, '案卷调整', '/example/document', '/example/document', 0, 6, NULL, 'interaction-outlined', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (195, 188, '预留1', NULL, NULL, 0, 7, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (196, 188, '预留2', NULL, NULL, 0, 8, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (197, 188, '预留3', NULL, NULL, 0, 9, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (198, 188, '预留4', NULL, NULL, 0, 10, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (199, 0, '获取授权', 'https://eleadmin.com/goods/9', NULL, 0, 9, NULL, 'sketch-outlined', 0, '{\"modal\": true}', '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:49:10', NULL, '2023-11-13 15:49:10', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (201, 0, '系统管理', 'javascript:;', NULL, 0, 1, NULL, 'layui-icon layui-icon-set', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (202, 201, '用户管理', '#/system/user', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (203, 202, '查询用户', NULL, NULL, 1, 1, 'sys:user:list', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (204, 202, '添加用户', NULL, NULL, 1, 2, 'sys:user:save', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (205, 202, '修改用户', NULL, NULL, 1, 3, 'sys:user:update', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (206, 202, '删除用户', NULL, NULL, 1, 4, 'sys:user:remove', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (207, 201, '角色管理', '#/system/role', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (208, 207, '查询角色', NULL, NULL, 1, 1, 'sys:role:list', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (209, 207, '添加角色', NULL, NULL, 1, 2, 'sys:role:save', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (210, 207, '修改角色', NULL, NULL, 1, 3, 'sys:role:update', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (211, 207, '删除角色', NULL, NULL, 1, 4, 'sys:role:remove', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (212, 201, '菜单管理', '#/system/authorities', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:24', NULL, '2023-11-13 15:49:24', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (213, 212, '查询菜单', NULL, NULL, 1, 1, 'sys:menu:list', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (214, 212, '添加菜单', NULL, NULL, 1, 2, 'sys:menu:save', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (215, 212, '修改菜单', NULL, NULL, 1, 3, 'sys:menu:update', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (216, 212, '删除菜单', NULL, NULL, 1, 4, 'sys:menu:remove', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (217, 201, '机构管理', '#/system/organization', NULL, 0, 4, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (218, 217, '查询机构', NULL, NULL, 1, 1, 'sys:org:list', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (219, 217, '添加机构', NULL, NULL, 1, 2, 'sys:org:save', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (220, 217, '修改机构', NULL, NULL, 1, 3, 'sys:org:update', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (221, 217, '删除机构', NULL, NULL, 1, 4, 'sys:org:remove', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (222, 201, '字典管理', '#/system/dictionary', NULL, 0, 5, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (223, 222, '查询字典', NULL, NULL, 1, 1, 'sys:dict:list', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (224, 222, '添加字典', NULL, NULL, 1, 2, 'sys:dict:save', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (225, 222, '修改字典', NULL, NULL, 1, 3, 'sys:dict:update', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (226, 222, '删除字典', NULL, NULL, 1, 4, 'sys:dict:remove', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (227, 201, '登录日志', '#/system/login-record', NULL, 0, 7, 'sys:login-record:list', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (228, 201, '操作日志', '#/system/oper-record', NULL, 0, 8, 'sys:operation-record:list', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (229, 201, '上传文件', NULL, NULL, 1, 9, 'sys:file:upload', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (230, 201, '修改个人密码', NULL, NULL, 1, 10, 'sys:auth:password', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (231, 201, '修改个人资料', NULL, NULL, 1, 11, 'sys:auth:user', NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (232, 0, 'Dashboard', 'javascript:;', NULL, 0, 0, NULL, 'layui-icon layui-icon-home', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (233, 232, '工作台', '#/console/workplace', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (234, 232, '控制台', '#/console/console', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (235, 232, '分析页', '#/console/dashboard', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (236, 0, '模板页面', 'javascript:;', NULL, 0, 2, NULL, 'layui-icon layui-icon-template', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:25', NULL, '2023-11-13 15:49:25', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (237, 236, '表单页', 'javascript:;', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (238, 237, '基础表单', '#/template/form/form-basic', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (239, 237, '复杂表单', '#/template/form/form-advance', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (240, 237, '分步表单', '#/template/form/form-step', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (241, 236, '表格页', 'javascript:;', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (242, 241, '数据表格', '#/template/table/table-basic', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (243, 241, '复杂表格', '#/template/table/table-advance', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (244, 241, '图片表格', '#/template/table/table-img', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (245, 241, '卡片列表', '#/template/table/table-card', NULL, 0, 4, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (246, 236, '错误页', 'javascript:;', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (247, 246, '500', '#/template/error/error-500', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (248, 246, '404', '#/template/error/error-404', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (249, 246, '403', '#/template/error/error-403', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (250, 236, '登录页', 'javascript:;', NULL, 0, 4, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (251, 250, '登录页', 'login.html', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (252, 250, '注册页', 'components/template/login/reg.html', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (253, 250, '忘记密码', 'components/template/login/forget.html', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (254, 236, '个人中心', '#/template/user-info', NULL, 0, 5, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (255, 0, '扩展组件', 'javascript:;', NULL, 0, 3, NULL, 'layui-icon layui-icon-component', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:26', NULL, '2023-11-13 15:49:26', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (256, 255, '常用组件', 'javascript:;', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (257, 256, '弹窗扩展', '#/plugin/basic/dialog', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (258, 256, '下拉菜单', '#/plugin/basic/dropdown', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (259, 256, '消息通知', '#/plugin/basic/notice', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (260, 256, '标签输入', '#/plugin/basic/tagsInput', NULL, 0, 4, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (261, 256, '级联选择', '#/plugin/basic/cascader', NULL, 0, 5, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (262, 256, '步骤条', '#/plugin/basic/steps', NULL, 0, 6, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (263, 255, '进阶组件', 'javascript:;', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (264, 263, '打印插件', '#/plugin/advance/printer', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (265, 263, '分割面板', '#/plugin/advance/split', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:27', NULL, '2023-11-13 15:49:27', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (266, 263, '表单扩展', '#/plugin/advance/formX', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (267, 263, '表格扩展', '#/plugin/advance/tableX', NULL, 0, 4, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (268, 263, '数据列表', '#/plugin/advance/dataGrid', NULL, 0, 5, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (269, 263, '鼠标右键', '#/plugin/advance/contextMenu', NULL, 0, 6, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (270, 255, '其他组件', 'javascript:;', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (271, 270, '圆形进度条', '#/plugin/other/circleProgress', '', 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (272, 270, '富文本编辑', '#/plugin/other/editor', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (273, 270, '鼠标滚轮', '#/plugin/other/mousewheel', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (274, 270, '更多组件', '#/plugin/other/other', NULL, 0, 4, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (275, 255, '更多扩展', '#/plugin/more', NULL, 0, 4, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (276, 0, '经典实例', 'javascript:;', NULL, 0, 4, NULL, 'layui-icon layui-icon-app', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (277, 276, '弹窗实例', '#/example/dialog', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (278, 276, '课程管理', '#/example/course', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:28', NULL, '2023-11-13 15:49:28', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (279, 276, '排课管理', '#/example/calendar', NULL, 0, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (280, 276, '添加试题', '#/example/question', NULL, 0, 4, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (281, 276, '文件管理', '#/example/file', NULL, 0, 5, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (282, 276, '表格CRUD', '#/example/table-crud', NULL, 0, 6, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (283, 276, '路由传参', '#/example/router-demo', NULL, 0, 7, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (284, 276, '多系统模式', 'side-more.html', NULL, 0, 8, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (285, 0, 'LayUI组件', 'javascript:;', NULL, 0, 5, NULL, 'layui-icon layui-icon-release', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (286, 285, '组件演示', '#/plugin/other/layui', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (287, 285, '开发文档', '#/layui/doc', 'https://eleadmin.com/doc/spa/', 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (288, 0, '多级菜单', 'javascript:;', NULL, 0, 6, NULL, 'layui-icon layui-icon-unlink', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (289, 288, '二级菜单', 'javascript:;', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (290, 289, '内嵌官网', '#/baidu', 'https://www.eleadmin.com', 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (291, 289, '三级菜单', 'javascript:;', NULL, 0, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:29', NULL, '2023-11-13 15:49:29', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (292, 291, '四级菜单', 'javascript:;', NULL, 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:30', NULL, '2023-11-13 15:49:30', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (293, 292, '五级菜单', '#/eleadmin-doc', 'https://eleadmin.com/goods/2', 0, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:30', NULL, '2023-11-13 15:49:30', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (294, 0, '一级菜单', '#/eleadmin', 'https://eleadmin.com/goods/3', 0, 7, NULL, 'layui-icon layui-icon-unlink', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:30', NULL, '2023-11-13 15:49:30', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (295, 0, '路由传参', '#/example/router-param', NULL, 0, 8, NULL, NULL, 1, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:49:30', NULL, '2023-11-13 15:49:30', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (301, 0, '系统管理', '/system', NULL, 0, 1, NULL, 'SettingOutlined', 0, '{\"props\": {\"badge\": {\"value\": \"New\", \"type\": \"warning\"}}, \"lang\": {\"zh_TW\": \"系統管理\", \"en\": \"System\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:48', NULL, '2024-05-15 16:15:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (302, 301, '用户管理', '/system/user', '/system/user', 0, 1, NULL, 'UserOutlined', 0, '{\"lang\": {\"zh_TW\": \"用戶管理\", \"en\": \"User\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:48', NULL, '2024-05-15 16:15:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (303, 302, '查询用户', NULL, NULL, 1, 1, 'sys:user:list', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:48', NULL, '2024-05-15 16:15:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (304, 302, '添加用户', NULL, NULL, 1, 2, 'sys:user:save', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:48', NULL, '2024-05-15 16:15:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (305, 302, '修改用户', NULL, NULL, 1, 3, 'sys:user:update', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:48', NULL, '2024-05-15 16:15:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (306, 302, '删除用户', NULL, NULL, 1, 4, 'sys:user:remove', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:48', NULL, '2024-05-15 16:15:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (307, 301, '角色管理', '/system/role', '/system/role', 0, 2, NULL, 'IdcardOutlined', 0, '{\"lang\": {\"zh_TW\": \"角色管理\", \"en\": \"Role\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:48', NULL, '2024-05-15 16:15:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (308, 307, '查询角色', NULL, NULL, 1, 1, 'sys:role:list', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:48', NULL, '2024-05-15 16:15:48', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (309, 307, '添加角色', NULL, NULL, 1, 2, 'sys:role:save', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (310, 307, '修改角色', NULL, NULL, 1, 3, 'sys:role:update', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (311, 307, '删除角色', NULL, NULL, 1, 4, 'sys:role:remove', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (312, 301, '菜单管理', '/system/menu', '/system/menu', 0, 3, NULL, 'AppstoreOutlined', 0, '{\"lang\": {\"zh_TW\": \"選單管理\", \"en\": \"Menu\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (313, 312, '查询菜单', NULL, NULL, 1, 1, 'sys:menu:list', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (314, 312, '添加菜单', NULL, NULL, 1, 2, 'sys:menu:save', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (315, 312, '修改菜单', NULL, NULL, 1, 3, 'sys:menu:update', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (316, 312, '删除菜单', NULL, NULL, 1, 4, 'sys:menu:remove', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (317, 301, '机构管理', '/system/organization', '/system/organization', 0, 4, NULL, 'CityOutlined', 0, '{\"hideFooter\":true, \"lang\": {\"zh_TW\": \"機构管理\", \"en\": \"Organization\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-19 17:03:41', 40, '2024-06-19 17:03:41', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (318, 317, '查询机构', NULL, NULL, 1, 1, 'sys:org:list', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (319, 317, '添加机构', NULL, NULL, 1, 2, 'sys:org:save', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (320, 317, '修改机构', NULL, NULL, 1, 3, 'sys:org:update', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (321, 317, '删除机构', NULL, NULL, 1, 4, 'sys:org:remove', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (322, 301, '字典管理', '/system/dictionary', '/system/dictionary', 0, 5, NULL, 'BookOutlined', 0, '{\"hideFooter\":true, \"lang\": {\"zh_TW\": \"字典管理\", \"en\": \"Dictionary\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (323, 322, '查询字典', NULL, NULL, 1, 1, 'sys:dict:list', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (324, 322, '添加字典', NULL, NULL, 1, 2, 'sys:dict:save', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (325, 322, '修改字典', NULL, NULL, 1, 3, 'sys:dict:update', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (326, 322, '删除字典', NULL, NULL, 1, 4, 'sys:dict:remove', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (327, 301, '登录日志', '/system/login-record', '/system/login-record', 0, 7, 'sys:login-record:list', 'CalendarOutlined', 0, '{\"lang\": {\"zh_TW\": \"登入日誌\", \"en\": \"LoginRecord\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (328, 301, '操作日志', '/system/operation-record', '/system/operation-record', 0, 8, 'sys:operation-record:list', 'LogOutlined', 0, '{\"lang\": {\"zh_TW\": \"操作日誌\", \"en\": \"OperationRecord\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (329, 301, '文件管理', '/system/file', '/system/file', 0, 6, NULL, 'FolderOutlined', 0, '{\"lang\": {\"zh_TW\": \"檔案管理\", \"en\": \"File\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (330, 329, '上传文件', NULL, NULL, 1, 1, 'sys:file:upload', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (331, 329, '删除文件', NULL, NULL, 1, 2, 'sys:file:remove', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (332, 329, '查看记录', NULL, NULL, 1, 3, 'sys:file:list', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:49', NULL, '2024-05-15 16:15:49', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (333, 302, '用户详情', '/system/user/details/:id', '/system/user/details', 0, 5, NULL, 'UserOutlined', 1, '{\"active\": \"/system/user\", \"lang\": {\"zh_TW\": \"用戶詳情\", \"en\": \"UserDetails\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-19 17:10:39', 40, '2024-05-19 17:10:40', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (334, 301, '修改个人密码', NULL, NULL, 1, 20, 'sys:auth:password', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-23 16:29:40', 40, '2024-05-23 16:29:41', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (335, 301, '修改个人资料', NULL, NULL, 1, 21, 'sys:auth:user', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-23 16:29:44', 40, '2024-05-23 16:29:45', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (336, 0, 'Dashboard', '/dashboard', NULL, 0, 0, NULL, 'HomeOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (337, 336, '工作台', '/dashboard/workplace', '/dashboard/workplace', 0, 1, NULL, 'DesktopOutlined', 0, '{\"lang\": {\"zh_TW\": \"工作臺\", \"en\": \"Workplace\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (338, 336, '分析页', '/dashboard/analysis', '/dashboard/analysis', 0, 2, NULL, 'AnalysisOutlined', 0, '{\"props\": {\"badge\": {\"value\": 1}}, \"lang\": {\"zh_TW\": \"分析頁\", \"en\": \"Analysis\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (339, 336, '监控页', '/dashboard/monitor', '/dashboard/monitor', 0, 3, NULL, 'DashboardOutlined', 0, '{\"lang\": {\"zh_TW\": \"監控頁\", \"en\": \"Monitor\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (340, 0, '表单页面', '/form', NULL, 0, 2, NULL, 'FormOutlined', 0, '{\"lang\": {\"zh_TW\": \"表單頁面\", \"en\": \"Form\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (341, 340, '基础表单', '/form/basic', '/form/basic', 0, 1, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"基礎表單\", \"en\": \"Basic Form\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (342, 340, '复杂表单', '/form/advanced', '/form/advanced', 0, 2, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"複雜表單\", \"en\": \"Advanced Form\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (343, 340, '分步表单', '/form/step', '/form/step', 0, 3, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"分步表單\", \"en\": \"Step Form\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (344, 0, '列表页面', '/list', NULL, 0, 3, NULL, 'TableOutlined', 0, '{\"props\": {\"hideTimeout\": 450}, \"lang\": {\"zh_TW\": \"清單頁面\", \"en\": \"List\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (345, 344, '基础列表', '/list/basic', '/list/basic', 0, 1, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"基礎清單\", \"en\": \"Basic List\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (346, 344, '复杂列表', '/list/advanced', '/list/advanced', 0, 2, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"複雜清單\", \"en\": \"Advanced List\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (347, 344, '卡片列表', '/list/card', '/list/card', 0, 3, NULL, 'LinkOutlined', 0, '{\"props\": {\"hideTimeout\": 100}, \"lang\": {\"zh_TW\": \"卡片清單\", \"en\": \"Card List\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (348, 347, '项目列表', '/list/card/project', '/list/card/project', 0, 1, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"項目清單\", \"en\": \"Project\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (349, 347, '应用列表', '/list/card/application', '/list/card/application', 0, 2, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"應用清單\", \"en\": \"Application\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (350, 347, '文章列表', '/list/card/article', '/list/card/article', 0, 3, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"文章清單\", \"en\": \"Article\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (351, 345, '添加用户', '/list/basic/add', '/list/basic/add', 0, 4, NULL, 'LinkOutlined', 1, '{\"active\": \"/list/basic\", \"lang\": {\"zh_TW\": \"添加用戶\", \"en\": \"Add User\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (352, 345, '修改用户', '/list/basic/edit/:id', '/list/basic/edit', 0, 4, NULL, 'LinkOutlined', 1, '{\"active\": \"/list/basic\", \"lang\": {\"zh_TW\": \"編輯用戶\", \"en\": \"Edit User\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:50', NULL, '2024-05-15 16:15:50', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (353, 340, '表单构建', '/form/build', '/form/build', 0, 4, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"表單構建\", \"en\": \"Form Build\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (354, 0, '结果页面', '/result', NULL, 0, 4, NULL, 'CheckCircleOutlined', 0, '{\"lang\": {\"zh_TW\": \"結果頁面\", \"en\": \"Result\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (355, 354, '成功页', '/result/success', '/result/success', 0, 1, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"成功頁\", \"en\": \"Success\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (356, 354, '失败页', '/result/fail', '/result/fail', 0, 2, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"失敗頁\", \"en\": \"Fail\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (357, 0, '异常页面', '/exception', NULL, 0, 5, NULL, 'WarningOutlined', 0, '{\"lang\": {\"zh_TW\": \"异常頁面\", \"en\": \"Exception\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (358, 357, '403', '/exception/403', '/exception/403', 0, 1, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (359, 357, '404', '/exception/404', '/exception/404', 0, 2, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (360, 357, '500', '/exception/500', '/exception/500', 0, 3, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (361, 0, '个人中心', '/user', NULL, 0, 6, NULL, 'ControlOutlined', 0, '{\"lang\": {\"zh_TW\": \"個人中心\", \"en\": \"User\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (362, 361, '我的资料', '/user/profile', '/user/profile', 0, 1, NULL, 'UserOutlined', 0, '{\"lang\": {\"zh_TW\": \"個人資料\", \"en\": \"Profile\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (363, 361, '我的消息', '/user/message', '/user/message', 0, 2, NULL, 'MessageOutlined', 0, '{\"lang\": {\"zh_TW\": \"我的消息\", \"en\": \"Message\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (364, 0, '扩展组件', '/extension', NULL, 0, 7, NULL, 'AppstoreAddOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (365, 364, '标签输入', '/extension/tag', '/extension/tag', 0, 3, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (366, 364, '高级弹窗', '/extension/modal', '/extension/modal', 0, 4, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (367, 364, '文件列表', '/extension/file', '/extension/file', 0, 5, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (368, 364, '图片上传', '/extension/upload', '/extension/upload', 0, 6, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (369, 364, '拖拽排序', '/extension/dragsort', '/extension/dragsort', 0, 24, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (370, 364, '消息提示', '/extension/message', '/extension/message', 0, 1, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (371, 364, '城市选择', '/extension/regions', '/extension/regions', 0, 26, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (372, 364, '打印组件', '/extension/printer', '/extension/printer', 0, 11, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (373, 364, '导入导出', '/extension/excel', '/extension/excel', 0, 27, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (374, 364, '下拉树', '/extension/tree-select', '/extension/tree-select', 0, 18, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (375, 364, '可选卡片', '/extension/check-card', '/extension/check-card', 0, 21, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (376, 364, '下拉表格', '/extension/table-select', '/extension/table-select', 0, 10, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (377, 364, '分割面板', '/extension/split', '/extension/split', 0, 7, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:51', NULL, '2024-05-15 16:15:51', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (378, 364, '视频播放', '/extension/player', '/extension/player', 0, 28, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (379, 364, '地图组件', '/extension/map', '/extension/map', 0, 25, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (380, 364, '二维码', '/extension/qr-code', '/extension/qr-code', 0, 20, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (381, 364, '条形码', '/extension/bar-code', '/extension/bar-code', 0, 19, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (382, 364, '富文本框', '/extension/editor', '/extension/editor', 0, 29, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (383, 364, 'markdown', '/extension/markdown', '/extension/markdown', 0, 30, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (384, 364, '头像组合', '/extension/avatar', '/extension/avatar', 0, 2, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (385, 364, '图标选择', '/extension/icon', '/extension/icon', 0, 8, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (386, 364, '文本组件', '/extension/text', '/extension/text', 0, 12, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (387, 364, '高级表格', '/extension/table', '/extension/table', 0, 9, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (388, 301, '左树右表', '/system/user2', '/system/user', 0, 9, NULL, 'UserOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 17:04:54', 40, '2024-06-20 17:04:54', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (389, 364, '标签页', '/extension/tabs', '/extension/tabs', 0, 16, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (390, 364, '步骤条', '/extension/steps', '/extension/steps', 0, 15, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (391, 364, '导航菜单', '/extension/menu', '/extension/menu', 0, 14, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (392, 364, '水印组件', '/extension/watermark', '/extension/watermark', 0, 22, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (393, 364, '引导组件', '/extension/tour', '/extension/tour', 0, 13, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (394, 364, '分段器', '/extension/segmented', '/extension/segmented', 0, 17, NULL, 'LinkOutlined', 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (395, 0, '内嵌页面', '/iframe', NULL, 0, 8, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"內嵌頁面\", \"en\": \"IFrame\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (396, 395, '官网', '/iframe/eleadmin', 'https://www.eleadmin.com', 0, 1, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"官網\", \"en\": \"Website\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (397, 395, '文档', '/iframe/eleadmin-doc', 'https://www.eleadmin.com/doc/eleadminplus/', 0, 2, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"檔案\", \"en\": \"Document\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (398, 0, '功能演示', '/example', '/example', 0, 9, NULL, 'CompassOutlined', 0, '{\"lang\": {\"zh_TW\": \"功能演示\", \"en\": \"Demo\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (399, 0, '获取授权', 'https://eleadmin.com/goods/11', NULL, 0, 10, NULL, 'ProtectOutlined', 0, '{\"lang\": {\"zh_TW\": \"獲取授權\", \"en\": \"Authorization\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3100, 344, '复杂路由', '/list/users', NULL, 0, 4, NULL, 'LinkOutlined', 0, '{\"props\": {\"hideTimeout\": 100}, \"lang\": {\"zh_TW\": \"複雜路由\", \"en\": \"Route Demo\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3101, 3100, '男用户', '/list/users/1', '/list/users', 0, 1, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"男用戶\", \"en\": \"Male Users\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3102, 3100, '女用户', '/list/users/2', '/list/users', 0, 2, NULL, 'LinkOutlined', 0, '{\"lang\": {\"zh_TW\": \"女用戶\", \"en\": \"Female Users\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3103, 3101, '男用户详情', '/list/users/details/1/:id', '/list/users/details', 0, 1, NULL, 'LinkOutlined', 1, '{\"active\": \"/list/users/1\", \"lang\": {\"zh_TW\": \"男用戶詳情\", \"en\": \"MaleUserDetails\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:52', NULL, '2024-05-15 16:15:52', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3104, 3102, '女用户详情', '/list/users/details/2/:id', '/list/users/details', 0, 1, NULL, 'LinkOutlined', 1, '{\"active\": \"/list/users/2\", \"lang\": {\"zh_TW\": \"女用戶詳情\", \"en\": \"FemaleUserDetails\"}}', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-15 16:15:53', NULL, '2024-05-15 16:15:53', 0, NULL);
INSERT INTO `sys_menu` (`id`, `parentId`, `title`, `path`, `component`, `menuType`, `sort`, `authority`, `icon`, `hide`, `meta`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (280971779886555263, 301, '流程设计', '/workflow', '/workflow', 0, 10, '', 'ConnectionOutlined', 0, '', '3a0e9b79-24bd-182a-522d-09067cf926ae', 40, '2024-05-23 16:30:44', 40, '2024-05-23 16:30:44', 0, NULL);
COMMIT;

-- ----------------------------
-- Table structure for sys_operation_record
-- ----------------------------
DROP TABLE IF EXISTS `sys_operation_record`;
CREATE TABLE `sys_operation_record` (
  `id` int NOT NULL AUTO_INCREMENT COMMENT '主键',
  `userName` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '用户',
  `nickname` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '用户',
  `module` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '操作模块',
  `description` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '操作功能',
  `url` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '请求地址',
  `requestMethod` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '请求方式',
  `method` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '调用方法',
  `params` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '请求参数',
  `result` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '返回结果',
  `error` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '异常信息',
  `comments` varchar(400) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '备注',
  `spendTime` int DEFAULT NULL COMMENT '消耗时间, 单位毫秒',
  `os` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '操作系统',
  `device` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '设备名称',
  `browser` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '浏览器类型',
  `ip` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'ip地址',
  `status` int NOT NULL DEFAULT '0' COMMENT '状态, 0成功, 1异常',
  `tenantId` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '租户id',
  `createdTime` datetime NOT NULL COMMENT '创建时间',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `user_id` (`userName`) USING BTREE,
  KEY `tenant_id` (`tenantId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='操作日志';

-- ----------------------------
-- Records of sys_operation_record
-- ----------------------------
BEGIN;
COMMIT;

-- ----------------------------
-- Table structure for sys_org
-- ----------------------------
DROP TABLE IF EXISTS `sys_org`;
CREATE TABLE `sys_org` (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '机构id',
  `parentId` bigint NOT NULL DEFAULT '0' COMMENT '上级id, 0是顶级',
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '机构名称',
  `fullname` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '机构全称',
  `code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '机构代码',
  `type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '机构类型',
  `leaderId` int DEFAULT NULL COMMENT '负责人id',
  `leader` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '负责人',
  `sort` int NOT NULL DEFAULT '1' COMMENT '排序号',
  `comments` varchar(400) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '备注',
  `tenantId` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '租户id',
  `creatorId` bigint DEFAULT NULL COMMENT '创建人',
  `createdTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `lastUpdaterId` bigint DEFAULT NULL COMMENT '更新人',
  `lastUpdatedTime` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `isDeleted` tinyint NOT NULL DEFAULT '0' COMMENT '是否删除',
  `deletionTime` datetime DEFAULT NULL COMMENT '删除时间',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `leader_id` (`leaderId`) USING BTREE,
  KEY `tenant_id` (`tenantId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=43 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='组织机构';

-- ----------------------------
-- Records of sys_org
-- ----------------------------
BEGIN;
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (1, 0, 'XXX公司', '武汉易云智科技有限公司', '91420111MA49EPRW0A', '1', NULL, '', 1, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:52', NULL, '2023-11-13 15:41:52', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (2, 1, '研发部', '研发部', NULL, '3', NULL, '', 2, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:52', NULL, '2023-11-13 15:41:52', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3, 2, '研发一组', '研发一组', NULL, '4', NULL, '', 3, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:52', NULL, '2023-11-13 15:41:52', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (4, 2, '研发二组', '研发二组', NULL, '4', NULL, '', 4, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:52', NULL, '2023-11-13 15:41:52', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (5, 2, '研发三组', '研发三组', NULL, '4', NULL, '', 5, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:52', NULL, '2023-11-13 15:41:52', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (6, 2, '研发四组', '研发四组', NULL, '4', NULL, '', 6, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:52', NULL, '2023-11-13 15:41:52', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (7, 1, '测试部', '测试部', NULL, '3', NULL, '', 7, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:53', NULL, '2023-11-13 15:41:53', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (8, 1, '设计部', '设计部', NULL, '3', NULL, '', 8, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:53', NULL, '2023-11-13 15:41:53', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (9, 1, '市场部', '市场部', NULL, '3', NULL, '', 9, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:53', NULL, '2023-11-13 15:41:53', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (10, 1, '运维部', '运维部', NULL, '3', NULL, '', 10, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:41:53', NULL, '2023-11-13 15:41:53', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (11, 0, 'XXX公司', '武汉易云智科技有限公司', NULL, '1', NULL, '', 1, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:41:59', NULL, '2023-11-13 15:41:59', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (12, 11, '研发部', '研发部', NULL, '3', NULL, '', 2, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:41:59', NULL, '2023-11-13 15:41:59', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (13, 12, '研发一组', '研发一组', NULL, '4', NULL, '', 3, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:41:59', NULL, '2023-11-13 15:41:59', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (14, 12, '研发二组', '研发二组', NULL, '4', NULL, '', 4, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:41:59', NULL, '2023-11-13 15:41:59', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (15, 12, '研发三组', '研发三组', NULL, '4', NULL, '', 5, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:41:59', NULL, '2023-11-13 15:41:59', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (16, 12, '研发四组', '研发四组', NULL, '4', NULL, '', 6, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:41:59', NULL, '2023-11-13 15:41:59', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (17, 11, '测试部', '测试部', NULL, '3', NULL, '', 7, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:41:59', NULL, '2023-11-13 15:41:59', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (18, 11, '设计部', '设计部', NULL, '3', NULL, '', 8, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:42:00', NULL, '2023-11-13 15:42:00', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (19, 11, '市场部', '市场部', NULL, '3', NULL, '', 9, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:42:00', NULL, '2023-11-13 15:42:00', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (20, 11, '运维部', '运维部', NULL, '3', NULL, '', 10, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:42:00', NULL, '2023-11-13 15:42:00', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (21, 0, 'XXX公司', '武汉易云智科技有限公司', '91420111MA49EPRW0A', '1', NULL, '', 1, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:08', NULL, '2023-11-13 15:42:08', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (22, 21, '研发部', '研发部', NULL, '3', NULL, '', 2, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:08', NULL, '2023-11-13 15:42:08', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (23, 22, '研发一组', '研发一组', NULL, '4', NULL, '', 3, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:08', NULL, '2023-11-13 15:42:08', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (24, 22, '研发二组', '研发二组', NULL, '4', NULL, '', 4, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:08', NULL, '2023-11-13 15:42:08', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (25, 22, '研发三组', '研发三组', NULL, '4', NULL, '', 5, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:08', NULL, '2023-11-13 15:42:08', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (26, 22, '研发四组', '研发四组', NULL, '4', NULL, '', 6, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:08', NULL, '2023-11-13 15:42:08', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (27, 21, '测试部', '测试部', NULL, '3', NULL, '', 7, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:08', NULL, '2023-11-13 15:42:08', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (28, 21, '设计部', 'UI设计部门', NULL, '3', NULL, '', 8, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:08', NULL, '2023-11-13 15:42:08', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (29, 21, '市场部', '市场部', NULL, '3', NULL, '', 9, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:09', NULL, '2023-11-13 15:42:09', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (30, 21, '运维部', '运维部', NULL, '3', NULL, '', 10, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:42:09', NULL, '2023-11-13 15:42:09', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (31, 0, 'XXX公司', '武汉易云智科技有限公司', '91420111MA49EPRW0A', '1', NULL, '', 1, '10', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2023-11-13 17:19:00', 40, '2023-11-13 17:19:00', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (32, 31, '研发部', '研发部', NULL, '3', NULL, '', 2, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2023-11-13 15:42:17', NULL, '2023-11-13 15:42:17', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (33, 32, '研发一组', '研发一组', NULL, '4', NULL, '', 3, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2023-11-13 15:42:18', NULL, '2023-11-13 15:42:18', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (34, 32, '研发二组', '研发二组', NULL, '4', NULL, '', 4, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2023-11-13 15:42:18', NULL, '2023-11-13 15:42:18', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (35, 32, '研发三组', '研发三组', NULL, '4', NULL, '', 5, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-05-19 17:22:16', 40, '2024-05-19 17:22:17', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (36, 32, '研发四组', '研发四组', NULL, '4', NULL, '', 6, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2023-11-13 15:42:18', NULL, '2023-11-13 15:42:18', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (37, 31, '测试部', '测试部', NULL, '3', NULL, '', 7, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2023-11-13 15:42:18', NULL, '2023-11-13 15:42:18', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (38, 31, '设计部', 'UI设计部门', NULL, '3', NULL, '', 8, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2023-11-13 15:42:18', NULL, '2023-11-13 15:42:18', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (39, 31, '市场部', '市场部', NULL, '3', NULL, '', 9, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2023-11-13 15:42:18', NULL, '2023-11-13 15:42:18', 0, NULL);
INSERT INTO `sys_org` (`id`, `parentId`, `name`, `fullname`, `code`, `type`, `leaderId`, `leader`, `sort`, `comments`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (40, 31, '运维部', '运维部', NULL, '3', NULL, '', 10, '', '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-19 17:23:21', 40, '2024-06-19 17:23:21', 0, NULL);
COMMIT;

-- ----------------------------
-- Table structure for sys_role
-- ----------------------------
DROP TABLE IF EXISTS `sys_role`;
CREATE TABLE `sys_role` (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '角色id',
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '角色名称',
  `code` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '角色标识',
  `comments` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '备注',
  `dataScope` int NOT NULL,
  `orgJson` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `tenantId` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '租户id',
  `creatorId` bigint DEFAULT NULL COMMENT '创建人',
  `createdTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `lastUpdaterId` bigint DEFAULT NULL COMMENT '更新人',
  `lastUpdatedTime` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `isDeleted` tinyint NOT NULL DEFAULT '0' COMMENT '是否删除',
  `deletionTime` datetime DEFAULT NULL COMMENT '删除时间',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `tenant_id` (`tenantId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='角色';

-- ----------------------------
-- Records of sys_role
-- ----------------------------
BEGIN;
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (1, '管理员', 'admin', '管理员', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:40:22', NULL, '2023-11-13 15:40:22', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (2, '普通用户', 'user', '普通用户', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:40:22', NULL, '2023-11-13 15:40:22', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3, '游客', 'guest', '游客', 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:40:22', NULL, '2023-11-13 15:40:22', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (4, '管理员', 'admin', '管理员', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:40:28', NULL, '2023-11-13 15:40:28', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (5, '普通用户', 'user', '普通用户', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:40:28', NULL, '2023-11-13 15:40:28', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (6, '游客', 'guest', '游客', 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:40:29', NULL, '2023-11-13 15:40:29', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (7, '管理员', 'admin', '管理员', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:40:35', NULL, '2023-11-13 15:40:35', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (8, '普通用户', 'user', '普通用户', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:40:35', NULL, '2023-11-13 15:40:35', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (9, '游客', 'guest', '游客', 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:40:35', NULL, '2023-11-13 15:40:35', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (10, '管理员', 'admin', '管理员', 1, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-19 17:03:05', 40, '2024-06-19 17:03:06', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (11, '普通用户', 'user', '普通用户', 2, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-19 20:24:00', 40, '2024-06-19 20:24:00', 0, NULL);
INSERT INTO `sys_role` (`id`, `name`, `code`, `comments`, `dataScope`, `orgJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (12, '游客', 'guest', '游客', 5, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-19 20:23:51', 40, '2024-06-19 20:23:52', 0, NULL);
COMMIT;

-- ----------------------------
-- Table structure for sys_role_menu
-- ----------------------------
DROP TABLE IF EXISTS `sys_role_menu`;
CREATE TABLE `sys_role_menu` (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '主键id',
  `roleId` bigint NOT NULL COMMENT '角色id',
  `menuId` bigint NOT NULL COMMENT '菜单id',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `FK_sys_role_permission_role` (`roleId`) USING BTREE,
  KEY `menu_id` (`menuId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=280984260313743555 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='角色权限';

-- ----------------------------
-- Records of sys_role_menu
-- ----------------------------
BEGIN;
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (315, 3, 40);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (316, 3, 41);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (317, 3, 42);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (318, 3, 43);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (338, 6, 140);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (339, 6, 141);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (340, 6, 142);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (341, 6, 143);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (594, 7, 201);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (595, 7, 202);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (596, 7, 203);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (597, 7, 207);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (598, 7, 208);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (599, 7, 212);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (600, 7, 213);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (601, 7, 217);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (602, 7, 218);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (603, 7, 222);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (604, 7, 223);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (605, 7, 227);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (606, 7, 228);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (607, 7, 232);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (608, 7, 233);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (609, 7, 234);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (610, 7, 235);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (611, 7, 236);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (612, 7, 237);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (613, 7, 238);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (614, 7, 239);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (615, 7, 240);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (616, 7, 241);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (617, 7, 242);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (618, 7, 243);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (619, 7, 244);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (620, 7, 245);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (621, 7, 246);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (622, 7, 247);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (623, 7, 248);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (624, 7, 249);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (625, 7, 250);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (626, 7, 251);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (627, 7, 252);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (628, 7, 253);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (629, 7, 254);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (630, 7, 255);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (631, 7, 256);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (632, 7, 257);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (633, 7, 258);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (634, 7, 259);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (635, 7, 260);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (636, 7, 261);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (637, 7, 262);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (638, 7, 263);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (639, 7, 264);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (640, 7, 265);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (641, 7, 266);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (642, 7, 267);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (643, 7, 268);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (644, 7, 269);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (645, 7, 270);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (646, 7, 271);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (647, 7, 272);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (648, 7, 273);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (649, 7, 274);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (650, 7, 275);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (651, 7, 276);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (652, 7, 277);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (653, 7, 278);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (654, 7, 279);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (655, 7, 280);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (656, 7, 281);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (657, 7, 282);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (658, 7, 283);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (659, 7, 284);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (660, 7, 285);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (661, 7, 286);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (662, 7, 287);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (663, 7, 288);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (664, 7, 289);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (665, 7, 290);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (666, 7, 291);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (667, 7, 292);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (668, 7, 293);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (669, 7, 294);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (670, 7, 295);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (671, 8, 201);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (672, 8, 202);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (673, 8, 203);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (674, 8, 207);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (675, 8, 208);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (676, 8, 212);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (677, 8, 213);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (678, 8, 217);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (679, 8, 218);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (680, 8, 222);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (681, 8, 223);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (682, 8, 227);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (683, 8, 228);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (684, 9, 236);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (685, 9, 237);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (686, 9, 238);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (687, 9, 239);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (688, 9, 240);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (689, 9, 254);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1655, 11, 337);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1656, 11, 338);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1657, 11, 301);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1658, 11, 302);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1659, 11, 303);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1660, 11, 304);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1661, 11, 305);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1662, 11, 306);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1663, 11, 333);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1664, 11, 307);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1665, 11, 308);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1666, 11, 309);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1667, 11, 310);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1668, 11, 311);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1669, 11, 312);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1670, 11, 313);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1671, 11, 314);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1672, 11, 315);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1673, 11, 316);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1674, 11, 317);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1675, 11, 318);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1676, 11, 319);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1677, 11, 320);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1678, 11, 321);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1679, 11, 322);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1680, 11, 323);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1681, 11, 324);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1682, 11, 325);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1683, 11, 326);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1684, 11, 329);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1685, 11, 330);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1686, 11, 331);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1687, 11, 332);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1688, 11, 327);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1689, 11, 328);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1690, 11, 334);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1691, 11, 335);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1692, 11, 395);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1693, 11, 396);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1694, 11, 397);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1695, 11, 399);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1696, 11, 336);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1697, 4, 136);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1698, 4, 137);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1699, 4, 138);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1700, 4, 139);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1701, 4, 101);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1702, 4, 102);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1703, 4, 103);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1704, 4, 104);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1705, 4, 105);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1706, 4, 106);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1707, 4, 133);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1708, 4, 107);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1709, 4, 108);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1710, 4, 109);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1711, 4, 110);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1712, 4, 111);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1713, 4, 112);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1714, 4, 113);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1715, 4, 114);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1716, 4, 115);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1717, 4, 116);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1718, 4, 122);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1719, 4, 123);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1720, 4, 124);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1721, 4, 125);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1722, 4, 126);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1723, 4, 117);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1724, 4, 118);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1725, 4, 119);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1726, 4, 120);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1727, 4, 121);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1728, 4, 129);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1729, 4, 130);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1730, 4, 131);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1731, 4, 132);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1732, 4, 127);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1733, 4, 128);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1734, 4, 134);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1735, 4, 135);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1736, 4, 140);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1737, 4, 141);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1738, 4, 142);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1739, 4, 143);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1740, 4, 144);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1741, 4, 145);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1742, 4, 151);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1743, 4, 152);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1744, 4, 153);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1745, 4, 146);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1746, 4, 147);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1747, 4, 148);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1748, 4, 149);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1749, 4, 150);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1750, 4, 154);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1751, 4, 155);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1752, 4, 156);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1753, 4, 157);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1754, 4, 158);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1755, 4, 159);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1756, 4, 160);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1757, 4, 161);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1758, 4, 162);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1759, 4, 163);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1760, 4, 164);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1761, 4, 165);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1762, 4, 166);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1763, 4, 167);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1764, 4, 168);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1765, 4, 169);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1766, 4, 170);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1767, 4, 171);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1768, 4, 172);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1769, 4, 173);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1770, 4, 174);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1771, 4, 176);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1772, 4, 178);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1773, 4, 179);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1774, 4, 180);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1775, 4, 181);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1776, 4, 182);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1777, 4, 183);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1778, 4, 184);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1779, 4, 185);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1780, 4, 186);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1781, 4, 187);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1782, 4, 188);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1783, 4, 189);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1784, 4, 191);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1785, 4, 192);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1786, 4, 193);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1787, 4, 190);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1788, 4, 194);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1789, 4, 199);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1790, 5, 137);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1791, 5, 136);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1792, 5, 138);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1793, 5, 101);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1794, 5, 102);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1795, 5, 103);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1796, 5, 104);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1797, 5, 105);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1798, 5, 106);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1799, 5, 133);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1800, 5, 107);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1801, 5, 108);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1802, 5, 109);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1803, 5, 110);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1804, 5, 111);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1805, 5, 112);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1806, 5, 113);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1807, 5, 114);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1808, 5, 115);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1809, 5, 116);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1810, 5, 122);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1811, 5, 123);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1812, 5, 124);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1813, 5, 125);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1814, 5, 126);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1815, 5, 117);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1816, 5, 118);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1817, 5, 119);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1818, 5, 120);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1819, 5, 121);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1820, 5, 129);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1821, 5, 130);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1822, 5, 131);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1823, 5, 132);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1824, 5, 127);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1825, 5, 128);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1826, 5, 134);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1827, 5, 135);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1828, 5, 191);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1829, 5, 188);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1830, 5, 192);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1831, 5, 199);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1832, 1, 36);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1833, 1, 37);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1834, 1, 38);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1835, 1, 39);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1836, 1, 1);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1837, 1, 2);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1838, 1, 3);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1839, 1, 4);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1840, 1, 5);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1841, 1, 6);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1842, 1, 33);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1843, 1, 7);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1844, 1, 8);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1845, 1, 9);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1846, 1, 10);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1847, 1, 11);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1848, 1, 12);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1849, 1, 13);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1850, 1, 14);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1851, 1, 15);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1852, 1, 16);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1853, 1, 17);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1854, 1, 18);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1855, 1, 19);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1856, 1, 20);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1857, 1, 21);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1858, 1, 22);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1859, 1, 23);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1860, 1, 24);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1861, 1, 25);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1862, 1, 26);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1863, 1, 29);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1864, 1, 30);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1865, 1, 31);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1866, 1, 32);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1867, 1, 27);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1868, 1, 28);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1869, 1, 34);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1870, 1, 35);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1871, 1, 40);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1872, 1, 41);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1873, 1, 42);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1874, 1, 43);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1875, 1, 44);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1876, 1, 45);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1877, 1, 51);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1878, 1, 52);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1879, 1, 53);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1880, 1, 46);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1881, 1, 47);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1882, 1, 48);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1883, 1, 49);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1884, 1, 50);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1885, 1, 54);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1886, 1, 55);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1887, 1, 56);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1888, 1, 57);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1889, 1, 58);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1890, 1, 59);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1891, 1, 60);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1892, 1, 61);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1893, 1, 62);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1894, 1, 63);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1895, 1, 64);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1896, 1, 65);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1897, 1, 66);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1898, 1, 67);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1899, 1, 68);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1900, 1, 69);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1901, 1, 70);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1902, 1, 71);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1903, 1, 72);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1904, 1, 73);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1905, 1, 74);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1906, 1, 75);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1907, 1, 76);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1908, 1, 77);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1909, 1, 78);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1910, 1, 79);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1911, 1, 80);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1912, 1, 82);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1913, 1, 83);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1914, 1, 84);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1915, 1, 85);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1916, 1, 86);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1917, 1, 87);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1918, 1, 88);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1919, 1, 89);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1920, 1, 97);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1921, 1, 98);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1922, 1, 90);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1923, 1, 91);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1924, 1, 92);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1925, 1, 93);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1926, 1, 94);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1927, 1, 95);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1928, 1, 96);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1929, 1, 99);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1930, 2, 37);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1931, 2, 38);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1932, 2, 1);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1933, 2, 2);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1934, 2, 3);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1935, 2, 4);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1936, 2, 5);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1937, 2, 6);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1938, 2, 33);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1939, 2, 7);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1940, 2, 8);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1941, 2, 9);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1942, 2, 10);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1943, 2, 11);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1944, 2, 12);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1945, 2, 13);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1946, 2, 14);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1947, 2, 15);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1948, 2, 16);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1949, 2, 17);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1950, 2, 18);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1951, 2, 19);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1952, 2, 20);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1953, 2, 21);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1954, 2, 22);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1955, 2, 23);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1956, 2, 24);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1957, 2, 25);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1958, 2, 26);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1959, 2, 29);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1960, 2, 30);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1961, 2, 31);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1962, 2, 32);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1963, 2, 27);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1964, 2, 28);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1965, 2, 34);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1966, 2, 35);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1967, 2, 93);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1968, 2, 94);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1969, 2, 99);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1970, 2, 36);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (1971, 2, 90);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743448, 10, 336);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743449, 10, 337);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743450, 10, 338);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743451, 10, 339);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743452, 10, 301);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743453, 10, 302);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743454, 10, 303);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743455, 10, 304);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743456, 10, 305);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743457, 10, 306);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743458, 10, 333);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743459, 10, 307);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743460, 10, 308);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743461, 10, 309);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743462, 10, 310);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743463, 10, 311);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743464, 10, 312);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743465, 10, 313);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743466, 10, 314);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743467, 10, 315);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743468, 10, 316);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743469, 10, 317);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743470, 10, 318);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743471, 10, 319);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743472, 10, 320);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743473, 10, 321);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743474, 10, 322);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743475, 10, 323);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743476, 10, 324);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743477, 10, 325);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743478, 10, 326);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743479, 10, 329);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743480, 10, 330);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743481, 10, 331);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743482, 10, 332);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743483, 10, 327);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743484, 10, 328);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743485, 10, 388);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743486, 10, 280971779886555263);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743487, 10, 334);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743488, 10, 335);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743489, 10, 340);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743490, 10, 341);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743491, 10, 342);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743492, 10, 343);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743493, 10, 353);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743494, 10, 344);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743495, 10, 345);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743496, 10, 351);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743497, 10, 352);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743498, 10, 346);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743499, 10, 347);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743500, 10, 348);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743501, 10, 349);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743502, 10, 350);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743503, 10, 3100);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743504, 10, 3101);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743505, 10, 3103);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743506, 10, 3102);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743507, 10, 3104);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743508, 10, 354);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743509, 10, 355);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743510, 10, 356);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743511, 10, 357);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743512, 10, 358);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743513, 10, 359);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743514, 10, 360);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743515, 10, 361);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743516, 10, 362);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743517, 10, 363);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743518, 10, 364);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743519, 10, 370);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743520, 10, 384);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743521, 10, 365);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743522, 10, 366);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743523, 10, 367);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743524, 10, 368);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743525, 10, 377);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743526, 10, 385);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743527, 10, 387);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743528, 10, 376);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743529, 10, 372);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743530, 10, 386);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743531, 10, 393);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743532, 10, 391);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743533, 10, 390);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743534, 10, 389);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743535, 10, 394);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743536, 10, 374);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743537, 10, 381);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743538, 10, 380);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743539, 10, 375);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743540, 10, 392);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743541, 10, 369);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743542, 10, 379);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743543, 10, 371);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743544, 10, 373);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743545, 10, 378);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743546, 10, 382);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743547, 10, 383);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743548, 10, 395);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743549, 10, 396);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743550, 10, 397);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743551, 10, 398);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743552, 10, 399);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743553, 12, 398);
INSERT INTO `sys_role_menu` (`id`, `roleId`, `menuId`) VALUES (280984260313743554, 12, 399);
COMMIT;

-- ----------------------------
-- Table structure for sys_role_org
-- ----------------------------
DROP TABLE IF EXISTS `sys_role_org`;
CREATE TABLE `sys_role_org` (
  `id` bigint NOT NULL,
  `roleId` bigint NOT NULL,
  `orgId` bigint NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `FK_sys_role_permission_role` (`roleId`) USING BTREE,
  KEY `menu_id` (`orgId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Records of sys_role_org
-- ----------------------------
BEGIN;
INSERT INTO `sys_role_org` (`id`, `roleId`, `orgId`) VALUES (280984267711037529, 11, 37);
INSERT INTO `sys_role_org` (`id`, `roleId`, `orgId`) VALUES (280984267711037530, 11, 31);
COMMIT;

-- ----------------------------
-- Table structure for sys_tenant
-- ----------------------------
DROP TABLE IF EXISTS `sys_tenant`;
CREATE TABLE `sys_tenant` (
  `id` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '租户id',
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '租户名称',
  `comments` varchar(400) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '备注',
  `creatorId` bigint DEFAULT NULL COMMENT '创建人',
  `createdTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `lastUpdaterId` bigint DEFAULT NULL COMMENT '更新人',
  `lastUpdatedTime` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `isDeleted` tinyint NOT NULL DEFAULT '0' COMMENT '是否删除',
  `deletionTime` datetime DEFAULT NULL COMMENT '删除时间',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='租户';

-- ----------------------------
-- Records of sys_tenant
-- ----------------------------
BEGIN;
INSERT INTO `sys_tenant` (`id`, `name`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES ('3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', 'EleAdmin', NULL, NULL, '2023-11-13 15:08:00', NULL, '2023-11-13 15:08:00', 0, NULL);
INSERT INTO `sys_tenant` (`id`, `name`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES ('3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', 'EleAdminPro', NULL, NULL, '2023-11-13 15:08:00', NULL, '2023-11-13 15:08:00', 0, NULL);
INSERT INTO `sys_tenant` (`id`, `name`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES ('3a0e9b79-24bd-1816-ba6c-652545d1e7a6', 'EasyWeb', NULL, NULL, '2023-11-13 15:08:00', NULL, '2023-11-13 15:08:00', 0, NULL);
INSERT INTO `sys_tenant` (`id`, `name`, `comments`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES ('3a0e9b79-24bd-182a-522d-09067cf926ae', 'EleAdminPlus', NULL, NULL, '2023-11-13 15:08:00', NULL, '2023-11-13 15:08:00', 0, NULL);
COMMIT;

-- ----------------------------
-- Table structure for sys_user
-- ----------------------------
DROP TABLE IF EXISTS `sys_user`;
CREATE TABLE `sys_user` (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '用户id',
  `userName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '账号',
  `password` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '密码',
  `nickname` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '昵称',
  `avatar` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '头像',
  `sex` int DEFAULT NULL COMMENT '性别',
  `phone` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '手机号',
  `email` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '邮箱',
  `emailVerified` int NOT NULL DEFAULT '0' COMMENT '邮箱是否验证, 0否, 1是',
  `realName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '真实姓名',
  `idCard` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '身份证号',
  `birthday` date DEFAULT NULL COMMENT '出生日期',
  `introduction` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '个人简介',
  `orgId` int DEFAULT NULL COMMENT '机构id',
  `orgName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `employeeNumber` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `status` int NOT NULL DEFAULT '0' COMMENT '状态, 0正常, 1冻结',
  `roleJson` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `tenantId` char(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '租户id',
  `creatorId` bigint DEFAULT NULL COMMENT '创建人',
  `createdTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `lastUpdaterId` bigint DEFAULT NULL COMMENT '更新人',
  `lastUpdatedTime` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `isDeleted` tinyint NOT NULL DEFAULT '0' COMMENT '是否删除',
  `deletionTime` datetime DEFAULT NULL COMMENT '删除时间',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `organization_id` (`orgId`) USING BTREE,
  KEY `tenant_id` (`tenantId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=53 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='用户';

-- ----------------------------
-- Records of sys_user
-- ----------------------------
BEGIN;
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (1, 'admin', 'e10adc3949ba59abbe56e057f20f883e', '管理员', 'https://cdn.eleadmin.com/20200610/avatar.jpg', 1, '12345678901', NULL, 0, NULL, NULL, '2021-05-21', '遗其欲，则心静！', 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:32', NULL, '2023-11-13 15:37:32', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (2, 'user01', 'e10adc3949ba59abbe56e057f20f883e', '用户一', NULL, 2, '12345678902', NULL, 0, NULL, NULL, NULL, NULL, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:32', NULL, '2023-11-13 15:37:32', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (3, 'user02', 'e10adc3949ba59abbe56e057f20f883e', '用户二', NULL, 1, '12345678903', NULL, 0, NULL, NULL, NULL, NULL, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:32', NULL, '2023-11-13 15:37:32', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (4, 'user03', 'e10adc3949ba59abbe56e057f20f883e', '用户三', NULL, 2, '12345678904', NULL, 0, NULL, NULL, NULL, NULL, 1, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:32', NULL, '2023-11-13 15:37:32', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (5, 'user04', 'e10adc3949ba59abbe56e057f20f883e', '用户四', NULL, 1, '12345678905', NULL, 0, NULL, NULL, NULL, NULL, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:32', NULL, '2023-11-13 15:37:32', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (6, 'user05', 'e10adc3949ba59abbe56e057f20f883e', '用户五', NULL, 2, '12345678906', NULL, 0, NULL, NULL, NULL, NULL, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:33', NULL, '2023-11-13 15:37:33', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (7, 'user06', 'e10adc3949ba59abbe56e057f20f883e', '用户六', NULL, 1, '12345678907', NULL, 0, NULL, NULL, NULL, NULL, 2, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:33', NULL, '2023-11-13 15:37:33', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (8, 'user07', 'e10adc3949ba59abbe56e057f20f883e', '用户七', NULL, 1, '12345678908', NULL, 0, NULL, NULL, NULL, NULL, 3, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:33', NULL, '2023-11-13 15:37:33', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (9, 'user08', 'e10adc3949ba59abbe56e057f20f883e', '用户八', NULL, 2, '12345678909', NULL, 0, NULL, NULL, NULL, NULL, 3, NULL, NULL, 1, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:33', NULL, '2023-11-13 15:37:33', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (10, 'user09', 'e10adc3949ba59abbe56e057f20f883e', '用户九', NULL, 2, '12345678910', NULL, 0, NULL, NULL, NULL, NULL, 4, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:33', NULL, '2023-11-13 15:37:33', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (11, 'user10', 'e10adc3949ba59abbe56e057f20f883e', '用户十', NULL, 1, '12345678911', NULL, 0, NULL, NULL, NULL, NULL, 4, NULL, NULL, 1, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:33', NULL, '2023-11-13 15:37:33', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (12, 'user11', 'e10adc3949ba59abbe56e057f20f883e', '用户十一', NULL, 1, '12345678912', NULL, 0, NULL, NULL, NULL, NULL, 5, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:33', NULL, '2023-11-13 15:37:33', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (13, 'user12', 'e10adc3949ba59abbe56e057f20f883e', '用户十二', NULL, 2, '12345678913', NULL, 0, NULL, NULL, NULL, NULL, 6, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1262-6b1b-1ce21096d6ca', NULL, '2023-11-13 15:37:33', NULL, '2023-11-13 15:37:33', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (14, 'admin', 'e10adc3949ba59abbe56e057f20f883e', '管理员', 'https://cdn.eleadmin.com/20200610/avatar.jpg', 1, '12345678901', NULL, 0, NULL, NULL, '2021-05-21', '遗其欲，则心静！', 11, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (15, 'user01', 'e10adc3949ba59abbe56e057f20f883e', '用户一', NULL, 2, '12345678902', NULL, 0, NULL, NULL, NULL, NULL, 11, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (16, 'user02', 'e10adc3949ba59abbe56e057f20f883e', '用户二', NULL, 1, '12345678903', NULL, 0, NULL, NULL, NULL, NULL, 11, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (17, 'user03', 'e10adc3949ba59abbe56e057f20f883e', '用户三', NULL, 2, '12345678904', NULL, 0, NULL, NULL, NULL, NULL, 11, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (18, 'user04', 'e10adc3949ba59abbe56e057f20f883e', '用户四', NULL, 1, '12345678905', NULL, 0, NULL, NULL, NULL, NULL, 12, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (19, 'user05', 'e10adc3949ba59abbe56e057f20f883e', '用户五', NULL, 2, '12345678906', NULL, 0, NULL, NULL, NULL, NULL, 12, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (20, 'user06', 'e10adc3949ba59abbe56e057f20f883e', '用户六', NULL, 1, '12345678907', NULL, 0, NULL, NULL, NULL, NULL, 12, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (21, 'user07', 'e10adc3949ba59abbe56e057f20f883e', '用户七', NULL, 1, '12345678908', NULL, 0, NULL, NULL, NULL, NULL, 13, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (22, 'user08', 'e10adc3949ba59abbe56e057f20f883e', '用户八', NULL, 2, '12345678909', NULL, 0, NULL, NULL, NULL, NULL, 13, NULL, NULL, 1, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (23, 'user09', 'e10adc3949ba59abbe56e057f20f883e', '用户九', NULL, 2, '12345678910', NULL, 0, NULL, NULL, NULL, NULL, 14, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (24, 'user10', 'e10adc3949ba59abbe56e057f20f883e', '用户十', NULL, 1, '12345678911', NULL, 0, NULL, NULL, NULL, NULL, 14, NULL, NULL, 1, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (25, 'user11', 'e10adc3949ba59abbe56e057f20f883e', '用户十一', NULL, 1, '12345678912', NULL, 0, NULL, NULL, NULL, NULL, 15, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (26, 'user12', 'e10adc3949ba59abbe56e057f20f883e', '用户十二', NULL, 2, '12345678913', NULL, 0, NULL, NULL, NULL, NULL, 16, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1798-b3a5-f9ea0b7740bb', NULL, '2023-11-13 15:37:45', NULL, '2023-11-13 15:37:45', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (27, 'admin', 'e10adc3949ba59abbe56e057f20f883e', '管理员', 'https://cdn.eleadmin.com/20200610/avatar.jpg', 2, '12345678901', 'eleadmin@eclouds.com', 0, NULL, NULL, '2021-05-21', '遗其欲，则心静！', 21, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (28, 'user01', 'e10adc3949ba59abbe56e057f20f883e', '用户一', NULL, 2, '12345678902', NULL, 0, NULL, NULL, NULL, NULL, 21, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (29, 'user02', 'e10adc3949ba59abbe56e057f20f883e', '用户二', NULL, 1, '12345678903', NULL, 0, NULL, NULL, NULL, NULL, 21, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (30, 'user03', 'e10adc3949ba59abbe56e057f20f883e', '用户三', NULL, 2, '12345678904', NULL, 0, NULL, NULL, NULL, NULL, 21, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (31, 'user04', 'e10adc3949ba59abbe56e057f20f883e', '用户四', NULL, 1, '12345678905', NULL, 0, NULL, NULL, NULL, NULL, 22, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (32, 'user05', 'e10adc3949ba59abbe56e057f20f883e', '用户五', NULL, 2, '12345678906', NULL, 0, NULL, NULL, NULL, NULL, 22, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (33, 'user06', 'e10adc3949ba59abbe56e057f20f883e', '用户六', NULL, 1, '12345678907', NULL, 0, NULL, NULL, NULL, NULL, 22, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (34, 'user07', 'e10adc3949ba59abbe56e057f20f883e', '用户七', NULL, 1, '12345678908', NULL, 0, NULL, NULL, NULL, NULL, 23, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (35, 'user08', 'e10adc3949ba59abbe56e057f20f883e', '用户八', NULL, 2, '12345678909', NULL, 0, NULL, NULL, NULL, NULL, 23, NULL, NULL, 1, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (36, 'user09', 'e10adc3949ba59abbe56e057f20f883e', '用户九', NULL, 2, '12345678910', NULL, 0, NULL, NULL, NULL, NULL, 24, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (37, 'user10', 'e10adc3949ba59abbe56e057f20f883e', '用户十', NULL, 1, '12345678911', NULL, 0, NULL, NULL, NULL, NULL, 24, NULL, NULL, 1, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (38, 'user11', 'e10adc3949ba59abbe56e057f20f883e', '用户十一', NULL, 1, '12345678912', NULL, 0, NULL, NULL, NULL, NULL, 25, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (39, 'user12', 'e10adc3949ba59abbe56e057f20f883e', '用户十二', NULL, 2, '12345678913', NULL, 0, NULL, NULL, NULL, NULL, 26, NULL, NULL, 0, NULL, '3a0e9b79-24bd-1816-ba6c-652545d1e7a6', NULL, '2023-11-13 15:37:58', NULL, '2023-11-13 15:37:58', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (40, 'admin', 'e10adc3949ba59abbe56e057f20f883e', '管理员', 'https://cdn.eleadmin.com/20200610/avatar.jpg', 2, '12345678901', 'eleadmin@eclouds.com', 0, NULL, NULL, '2021-05-21', '遗其欲，则心静！', 31, 'XXX公司', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:53:26', 40, '2024-06-20 10:53:27', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (41, 'user01', 'e10adc3949ba59abbe56e057f20f883e', '用户一', 'https://cdn.eleadmin.com/20200610/avatar.jpg', 2, '12345678902', NULL, 0, NULL, NULL, NULL, NULL, 31, 'XXX公司', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:53:21', 40, '2024-06-20 10:53:21', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (42, 'user02', 'e10adc3949ba59abbe56e057f20f883e', '用户二', NULL, 1, '12345678903', NULL, 0, NULL, NULL, NULL, NULL, 31, 'XXX公司', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:53:16', 40, '2024-06-20 10:53:16', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (43, 'user03', 'e10adc3949ba59abbe56e057f20f883e', '用户三', NULL, 2, '12345678904', NULL, 0, NULL, NULL, NULL, NULL, 31, 'XXX公司', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:53:10', 40, '2024-06-20 10:53:10', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (44, 'user04', 'e10adc3949ba59abbe56e057f20f883e', '用户四', NULL, 1, '12345678905', NULL, 0, NULL, NULL, NULL, NULL, 40, '运维部', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:52:59', 40, '2024-06-20 10:52:59', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (45, 'user05', 'e10adc3949ba59abbe56e057f20f883e', '用户五', NULL, 2, '12345678906', NULL, 0, NULL, NULL, NULL, NULL, 39, '市场部', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:52:53', 40, '2024-06-20 10:52:53', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (46, 'user06', 'e10adc3949ba59abbe56e057f20f883e', '用户六', NULL, 1, '12345678907', NULL, 0, NULL, NULL, NULL, NULL, 38, '设计部', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:52:47', 40, '2024-06-20 10:52:48', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (47, 'user07', 'e10adc3949ba59abbe56e057f20f883e', '用户七', NULL, 1, '12345678908', NULL, 0, NULL, NULL, NULL, NULL, 37, '测试部', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:52:43', 40, '2024-06-20 10:52:43', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (48, 'user08', 'e10adc3949ba59abbe56e057f20f883e', '用户八', NULL, 2, '12345678909', NULL, 0, NULL, NULL, NULL, NULL, 32, '研发部', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:52:33', 40, '2024-07-18 01:12:25', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (49, 'user09', 'e10adc3949ba59abbe56e057f20f883e', '用户九', NULL, 2, '12345678910', NULL, 0, NULL, NULL, NULL, NULL, 36, '研发四组', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:52:28', 40, '2024-06-20 10:52:29', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (50, 'user10', 'e10adc3949ba59abbe56e057f20f883e', '用户十', NULL, 1, '12345678911', NULL, 0, NULL, NULL, NULL, NULL, 35, '研发三组', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:52:21', 40, '2024-06-20 10:52:21', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (51, 'user11', 'e10adc3949ba59abbe56e057f20f883e', '用户十一', NULL, 1, '12345678912', NULL, 0, NULL, NULL, NULL, NULL, 34, '研发二组', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 10:52:16', 40, '2024-06-20 10:52:17', 0, NULL);
INSERT INTO `sys_user` (`id`, `userName`, `password`, `nickname`, `avatar`, `sex`, `phone`, `email`, `emailVerified`, `realName`, `idCard`, `birthday`, `introduction`, `orgId`, `orgName`, `employeeNumber`, `status`, `roleJson`, `tenantId`, `creatorId`, `createdTime`, `lastUpdaterId`, `lastUpdatedTime`, `isDeleted`, `deletionTime`) VALUES (52, 'user12', 'E10ADC3949BA59ABBE56E057F20F883E', '用户十二', NULL, 2, '12345678913', NULL, 0, NULL, NULL, NULL, NULL, 37, '仓储部', NULL, 0, NULL, '3a0e9b79-24bd-182a-522d-09067cf926ae', NULL, '2024-06-20 11:20:43', 40, '2024-07-14 17:10:26', 0, '2024-05-16 14:54:59');
COMMIT;

-- ----------------------------
-- Table structure for sys_user_role
-- ----------------------------
DROP TABLE IF EXISTS `sys_user_role`;
CREATE TABLE `sys_user_role` (
  `id` bigint NOT NULL AUTO_INCREMENT COMMENT '主键id',
  `userId` bigint NOT NULL COMMENT '用户id',
  `roleId` bigint NOT NULL COMMENT '角色id',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `FK_sys_user_role` (`userId`) USING BTREE,
  KEY `FK_sys_user_role_role` (`roleId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4289196781113196615 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='用户角色';

-- ----------------------------
-- Records of sys_user_role
-- ----------------------------
BEGIN;
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (1, 1, 1);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (2, 2, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (3, 3, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (4, 4, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (5, 5, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (6, 6, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (7, 7, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (8, 8, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (9, 9, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (10, 10, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (11, 11, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (12, 12, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (13, 13, 2);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (14, 14, 4);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (15, 15, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (16, 16, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (17, 17, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (18, 18, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (19, 19, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (20, 20, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (21, 21, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (22, 22, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (23, 23, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (24, 24, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (25, 25, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (26, 26, 5);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (27, 27, 7);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (28, 28, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (29, 29, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (30, 30, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (31, 31, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (32, 32, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (33, 33, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (34, 34, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (35, 35, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (36, 36, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (37, 37, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (38, 38, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (39, 39, 8);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037535, 51, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037536, 50, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037537, 49, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037539, 47, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037540, 46, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037541, 45, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037542, 44, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037544, 43, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037545, 42, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037546, 41, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280984267711037547, 40, 10);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (280994289743908934, 48, 11);
INSERT INTO `sys_user_role` (`id`, `userId`, `roleId`) VALUES (4289196781113196614, 52, 11);
COMMIT;

SET FOREIGN_KEY_CHECKS = 1;
