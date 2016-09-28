#include <MadgwickAHRS.h>
#include "MPU9250.h"

Madgwick filter; // initialise Madgwick object
int ax, ay, az;
int gx, gy, gz;
float yaw;
float pitch;
float roll;
int factor = 1; // variable by which to divide gyroscope values, used to control sensitivity
// note that an increased baud rate requires an increase in value of factor

int calibrateOffsets = 1; // int to determine whether calibration takes place or not

MPU9250 imu;

unsigned long timer;

void setup() {

  // power the MPU-9250 with analog pins
  pinMode(A2, OUTPUT);
  digitalWrite(A2, LOW);
  pinMode(A3, OUTPUT);
  digitalWrite(A3, HIGH);
  // let the MPU-9250 stabilize
  delay(1000);
  
  Wire.begin();
  // initialize Serial communication
  Serial.begin(115200);
  byte c = imu.readByte(MPU9250_ADDRESS, WHO_AM_I_MPU9250);
  Serial.print("MPU9250 "); Serial.print("I AM "); Serial.print(c, HEX);
  Serial.print(" I should be "); Serial.println(0x71, HEX);
  byte d = imu.readByte(AK8963_ADDRESS, WHO_AM_I_AK8963);
  Serial.print("AK8963 "); Serial.print("I AM "); Serial.print(d, HEX);
  Serial.print(" I should be "); Serial.println(0x48, HEX);
  imu.calibrateMPU9250(imu.gyroBias, imu.accelBias);
  imu.initMPU9250();
  imu.initAK8963(imu.magCalibration);
  Serial.println("MPU9250 initialized for active data mode....");
}

void loop() {
  if (imu.readByte(MPU9250_ADDRESS, INT_STATUS) & 0x01) {
    imu.readAccelData(imu.accelCount);
    imu.getAres();
    imu.ax = (float)imu.accelCount[0]*imu.aRes; // - accelBias[0];
    imu.ay = (float)imu.accelCount[1]*imu.aRes; // - accelBias[1];
    imu.az = (float)imu.accelCount[2]*imu.aRes; // - accelBias[2];
    
    imu.readGyroData(imu.gyroCount);
    imu.getGres();
    imu.gx = (float)imu.gyroCount[0]*imu.gRes;
    imu.gy = (float)imu.gyroCount[1]*imu.gRes;
    imu.gz = (float)imu.gyroCount[2]*imu.gRes;
    
    imu.readMagData(imu.magCount);
    imu.getMres();
    imu.mx = (float)imu.magCount[0]*imu.mRes*imu.magCalibration[0] -
               imu.magbias[0];
    imu.my = (float)imu.magCount[1]*imu.mRes*imu.magCalibration[1] -
               imu.magbias[1];
    imu.mz = (float)imu.magCount[2]*imu.mRes*imu.magCalibration[2] -
               imu.magbias[2];
    
    imu.updateTime();
  }

  // use function from MagdwickAHRS.h to return quaternions
  filter.updateIMU(imu.gx / factor, imu.gy / factor, imu.gz / factor, imu.ax, imu.ay, imu.az);

  // functions to find yaw roll and pitch from quaternions
  yaw = filter.getYaw();
  roll = filter.getRoll();
  pitch = filter.getPitch();

  unsigned long currentTimer = millis();

  if (currentTimer - timer >= 50) {
    Serial.print(yaw);
  Serial.print(";");
  Serial.print(pitch);
  Serial.print(";");
  Serial.println(roll);
  timer = currentTimer;
  }
  
}
