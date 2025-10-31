namespace MostlyLucid.EufySecurity.Common;

/// <summary>
/// Types of Eufy security devices
/// </summary>
public enum DeviceType
{
    Station = 0,
    Camera = 1,
    Sensor = 2,
    Floodlight = 3,
    CameraE = 4,
    Doorbell = 5,
    BatteryDoorbell = 7,
    Camera2C = 8,
    Camera2 = 9,
    MotionSensor = 10,
    Keypad = 11,
    Camera2Pro = 14,
    Camera2CPro = 15,
    BatteryDoorbell2 = 16,
    HB3 = 18,
    Camera3 = 19,
    Camera3C = 23,
    Professional247 = 24, // T8600
    MiniBaseChime = 25,
    Camera3Pro = 26,
    IndoorCamera = 30,
    IndoorPTCamera = 31,
    SoloCamera = 32,
    SoloCameraPro = 33,
    IndoorCamera1080 = 34,
    IndoorPTCamera1080 = 35,
    FloodlightCamera8422 = 37,
    FloodlightCamera8423 = 38,
    FloodlightCamera8424 = 39,
    IndoorOutdoorCamera1080PNoLight = 44,
    IndoorOutdoorCamera2K = 45,
    IndoorOutdoorCamera1080P = 46,
    FloodlightCamera8425 = 47,
    OutdoorPTCamera = 48, // S340
    LockBLE = 50,
    LockWifi = 51,
    LockBLENoFinger = 52,
    LockWifiNoFinger = 53,
    Lock8503 = 54, // Smart Lock R10
    Lock8530 = 55,
    Lock85A3 = 56,
    Lock8592 = 57,
    Lock8504 = 58, // Smart Lock R20
    SoloCameraSpotlight1080 = 60,
    SoloCameraSpotlight2K = 61,
    SoloCameraSpotlightSolar = 62,
    SoloCameraSolar = 63,
    SoloCameraC210 = 64,
    FloodlightCamera8426 = 87, // E30
    SoloCameraE30 = 88,
    SmartDrop = 90,
    BatteryDoorbellPlus = 91,
    DoorbellSolo = 93,
    BatteryDoorbellPlusE340 = 94,
    BatteryDoorbellC30 = 95,
    BatteryDoorbellC31 = 96,
    IndoorCostDownCamera = 100,
    CameraGun = 101,
    CameraSnail = 102,
    IndoorPTCameraS350 = 104,
    IndoorPTCameraE30 = 105,
    CameraFG = 110, // T8150
    CameraGarageT8453Common = 131,
    CameraGarageT8452 = 132,
    CameraGarageT8453 = 133,
    SmartSafe7400 = 140,
    SmartSafe7401 = 141,
    SmartSafe7402 = 142,
    SmartSafe7403 = 143,
    WallLightCam = 151,
    SmartTrackLink = 157, // T87B0
    SmartTrackCard = 159, // T87B2
    Lock8502 = 180,
    Lock8506 = 184,
    WallLightCam81A0 = 10005,
    IndoorPTCameraC220 = 10008, // T8W11C
    IndoorPTCameraC210 = 10009 // T8419 / T8W11P?
}
