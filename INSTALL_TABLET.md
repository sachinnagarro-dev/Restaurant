# Android Tablet Kiosk Installation Guide

This guide covers setting up Android tablets as dedicated kiosks for the TableOrder restaurant system. The tablet app is a React-based Progressive Web App (PWA) that provides table-side ordering functionality.

## 📱 Recommended Hardware

### Tablet Specifications
- **Screen Size**: 8-10 inches (optimal for restaurant tables)
- **RAM**: 4GB minimum, 6GB recommended
- **Storage**: 32GB minimum, 64GB recommended
- **Android Version**: Android 8.0+ (API level 26+)
- **WiFi**: 802.11n/ac support
- **Battery**: 5000mAh+ or continuous power adapter

### Recommended Models (India)
- **Samsung Galaxy Tab A8** (10.5", 4GB RAM) - ₹15,000-20,000 - Budget option
- **Samsung Galaxy Tab S6 Lite** (10.4", 4GB RAM) - ₹25,000-30,000 - Mid-range
- **Lenovo Tab P11** (11", 6GB RAM) - ₹20,000-25,000 - Premium option
- **Amazon Fire HD 10** (10.1", 3GB RAM) - ₹12,000-15,000 - Budget with custom setup
- **Realme Pad** (10.4", 4GB RAM) - ₹15,000-18,000 - Value option
- **iQOO Pad** (11", 8GB RAM) - ₹30,000-35,000 - High-end option

## 🔧 Pre-Installation Setup

### 1. Factory Reset (Recommended)
```bash
# For fresh installation, perform factory reset
Settings → System → Reset → Factory data reset
```

### 2. Disable Automatic Updates
```
Settings → System → Advanced → System update → Auto-download over Wi-Fi → OFF
Settings → Apps → Google Play Store → Disable auto-updates
```

### 3. Developer Options
```
Settings → About tablet → Tap "Build number" 7 times
Settings → Developer options → Enable "Stay awake" (while charging)
```

## 📶 Network Configuration

### 1. WiFi Setup
Create a dedicated restaurant network:

**Router Configuration:**
- **SSID**: `Restaurant-Kiosk` (or your preferred name)
- **Security**: WPA2-PSK (minimum)
- **Channel**: Use 5GHz band for better performance (avoid crowded 2.4GHz in India)
- **DHCP**: Reserve IP addresses for tablets (e.g., 192.168.1.100-199)
- **ISP**: Consider Jio, Airtel, or BSNL for reliable business internet
- **Backup**: Mobile hotspot as fallback (Jio/Airtel 4G/5G)

**Tablet WiFi Setup:**
```
Settings → Network & Internet → Wi-Fi
1. Connect to "Restaurant-Kiosk" network
2. Enter password
3. Advanced → Keep Wi-Fi on during sleep → Always
4. Advanced → Wi-Fi frequency band → 5 GHz (if available)
```

### 2. Network Optimization
```
Settings → Network & Internet → Wi-Fi → [Your Network] → Advanced
- Keep Wi-Fi on during sleep: Always
- Wi-Fi frequency band: 5 GHz
- MAC address type: Use device MAC
```

## 📱 App Installation Methods

### Method 1: Progressive Web App (PWA) - Recommended

#### Step 1: Access the App
1. Open Chrome browser on tablet
2. Navigate to your restaurant's tablet app URL:
   ```
   https://your-restaurant-domain.com/tablet
   ```

#### Step 2: Install as PWA
1. Tap the **"Add to Home Screen"** prompt (or menu → "Add to Home Screen")
2. Customize app name: "Restaurant Order"
3. Tap **"Add"**

#### Step 3: Configure PWA Settings
```
Chrome → Settings → Site Settings → [Your App]
- Notifications: Allow
- Camera: Allow (if needed for QR codes)
- Location: Allow (if needed)
- Pop-ups: Allow
```

### Method 2: APK Installation (Alternative)

If you have a compiled APK:

#### Enable Unknown Sources
```
Settings → Security → Install unknown apps → Chrome → Allow
```

#### Install APK
1. Download APK to tablet
2. Open file manager → Tap APK file
3. Follow installation prompts
4. Grant necessary permissions

## 🔒 Kiosk Mode Configuration

### Option 1: Screen Pinning (No MDM Required)

#### Enable Screen Pinning
```
Settings → Security → Screen pinning → ON
Settings → Security → Screen pinning → Ask for PIN before unpinning → ON
```

#### Pin the App
1. Open the restaurant app
2. Tap **Overview** button (recent apps)
3. Tap **Pin** icon on the app
4. Confirm pinning

#### Unpinning (for maintenance)
- Hold **Back** + **Overview** buttons simultaneously
- Enter PIN when prompted

### Option 2: Device Owner Mode (Advanced)

#### Prerequisites
- Factory reset device
- Enable Developer Options
- Enable USB Debugging

#### Set Device Owner via ADB
```bash
# Connect tablet via USB
adb devices

# Set your app as device owner
adb shell dpm set-device-owner com.yourcompany.restaurant/.MainActivity

# Configure lock task mode
adb shell dpm set-lock-task-packages com.yourcompany.restaurant
```

#### Configure Lock Task Mode
```bash
# Enable lock task mode for your app
adb shell am start -a android.intent.action.MAIN -n com.yourcompany.restaurant/.MainActivity --activity-clear-top
```

### Option 3: MDM Solution (Enterprise)

For multiple tablets, consider MDM solutions:
- **Android Enterprise** (Google)
- **Hexnode** (affordable)
- **Kandji** (Apple-focused but supports Android)
- **Miradore** (free tier available)

## 🔌 Hardware Setup

### 1. Mounting Options

#### Table Mounts
- **Adjustable tablet stands**: ₹1,500-3,500
- **Wall-mounted brackets**: ₹2,500-6,000
- **Counter-top stands**: ₹1,000-3,000
- **Floor stands**: ₹4,000-12,000

#### Recommended Mounts
- **VESA-compatible mounts** for professional look
- **Anti-theft locks** for security
- **Cable management** for clean appearance

### 2. Power Management

#### Always-On Configuration
```
Settings → Display → Sleep → Never (while charging)
Settings → Battery → Battery optimization → [Your App] → Don't optimize
```

#### Charging Setup
- Use **high-quality USB-C cables** (3A+ rating)
- **Wall adapters**: 15W+ recommended (ensure 220V compatibility)
- **Power strips**: Surge-protected, labeled outlets (Indian 3-pin plugs)
- **Backup power**: UPS for critical tablets (consider frequent power cuts in India)
- **Voltage stabilizer**: Recommended for areas with voltage fluctuations

### 3. Screen Protection
- **Tempered glass screen protectors**
- **Anti-glare films** for bright environments
- **Cleaning supplies**: Microfiber cloths, screen cleaner

## ⚙️ App Configuration

### 1. Initial Setup
1. Open the restaurant app
2. Configure restaurant settings:
   - Restaurant name
   - Table numbers
   - Menu categories
   - Payment methods (UPI, Cards, Cash)
   - Currency: Indian Rupees (₹)
   - Language: Hindi/English/Tamil/Telugu (as per location)

### 2. Offline Mode Setup
```
App Settings → Offline Mode → Enable
App Settings → Cache Menu → Enable
App Settings → Queue Orders → Enable
```

### 3. Display Settings
```
Settings → Display → Font size → Large
Settings → Display → Display size → Large
Settings → Accessibility → Touch & hold delay → Short
```

## 🔧 Maintenance & Troubleshooting

### Common Issues

#### 1. Network Connectivity Problems
**Symptoms**: App shows "No connection" or fails to load
**Solutions**:
```
1. Check WiFi connection
   - Settings → Network & Internet → Wi-Fi
   - Verify connected to correct network
   - Test with other devices

2. Restart network services
   - Settings → Network & Internet → Wi-Fi → Forget network
   - Reconnect to network
   - Restart tablet if needed

3. Check router/access point
   - Verify router is online
   - Check for firmware updates
   - Test with other devices
```

#### 2. App Not Loading
**Symptoms**: Blank screen, app crashes, or won't start
**Solutions**:
```
1. Clear app cache
   - Settings → Apps → [Restaurant App] → Storage → Clear Cache

2. Restart app
   - Force close app
   - Clear from recent apps
   - Reopen app

3. Reinstall app
   - Uninstall app
   - Clear browser data (if PWA)
   - Reinstall from source
```

#### 3. App Updates
**Symptoms**: App shows outdated menu or features
**Solutions**:
```
For PWA:
1. Hard refresh: Ctrl+Shift+R (or Cmd+Shift+R)
2. Clear browser cache
3. Check for service worker updates

For APK:
1. Download new APK
2. Install over existing app
3. Restart app
```

#### 4. Cached Orders Recovery
**Symptoms**: Orders stuck in queue, not syncing
**Solutions**:
```
1. Check network connection
2. Force sync: App Settings → Sync Now
3. Clear offline queue: App Settings → Clear Queue
4. Restart app
5. Check backend server status
```

#### 5. Tablet Reboot Issues
**Symptoms**: Tablet won't start, stuck on logo
**Solutions**:
```
1. Hard reset: Hold Power + Volume Down for 10+ seconds
2. Recovery mode: Hold Power + Volume Up + Volume Down
3. Factory reset (last resort)
4. Check power adapter and cable
```

### 6. Performance Issues
**Symptoms**: Slow app, laggy interface
**Solutions**:
```
1. Restart tablet
2. Clear app cache
3. Close other apps
4. Check available storage
5. Update app if available
```

## 📋 Maintenance Checklist

### Daily
- [ ] Check all tablets are online
- [ ] Verify app is running correctly
- [ ] Test order placement
- [ ] Check network connectivity

### Weekly
- [ ] Clean tablet screens
- [ ] Check for app updates
- [ ] Verify backup power systems
- [ ] Test offline functionality

### Monthly
- [ ] Update app if new version available
- [ ] Check tablet storage space
- [ ] Clean tablet cases/mounts
- [ ] Verify network performance
- [ ] Test emergency procedures

### Quarterly
- [ ] Full system backup
- [ ] Security updates
- [ ] Hardware inspection
- [ ] Performance optimization
- [ ] Staff training refresh

## 🚨 Emergency Procedures

### Complete System Failure
1. **Immediate**: Switch to manual ordering
2. **Short-term**: Use backup tablets if available
3. **Medium-term**: Contact IT support
4. **Long-term**: Implement redundancy measures

### Network Outage
1. **Check**: Router/switch power and connections
2. **Restart**: Network equipment
3. **Verify**: Internet connection
4. **Test**: Tablet connectivity
5. **Fallback**: Use mobile hotspot if available

### App Corruption
1. **Restart**: Tablet completely
2. **Clear**: App cache and data
3. **Reinstall**: App from source
4. **Restore**: Configuration settings
5. **Test**: Full functionality

## 📞 Support Contacts

### Technical Support
- **IT Department**: [Your IT contact]
- **App Developer**: [Developer contact]
- **Hardware Vendor**: [Tablet supplier]

### Emergency Contacts
- **Restaurant Manager**: [Manager contact]
- **IT Emergency**: [24/7 support number]
- **Backup System**: [Alternative ordering method]

## 📚 Additional Resources

### Documentation
- [Tablet App User Manual](./docs/tablet-user-guide.md)
- [Network Setup Guide](./docs/network-setup.md)
- [Troubleshooting Guide](./docs/troubleshooting.md)

### Training Materials
- [Staff Training Videos](./training/)
- [Quick Reference Cards](./docs/quick-reference.md)
- [Emergency Procedures](./docs/emergency-procedures.md)

---

**Last Updated**: [Current Date]
**Version**: 1.0
**Maintained by**: [Your IT Team]

For additional support or questions, contact your IT department or refer to the main project documentation.
