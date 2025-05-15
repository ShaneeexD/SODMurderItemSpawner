import React, { useState } from 'react';
import { 
  Typography, 
  Paper, 
  Divider, 
  TextField,
  Button,
  Box,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Alert,
  Snackbar,
  Stack
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import EventSelector from './EventSelector';
import ItemSelector from './ItemSelector';
import LocationSelector from './LocationSelector';
import OwnershipSelector from './OwnershipSelector';
import OutputDisplay from './OutputDisplay';
import { SpawnLocationType, BelongsTo, SubLocationTypeBuildingEntrances } from '../models/configTypes';
import type { SpawnRule, ConfigFile } from '../models/configTypes';

const ConfigForm: React.FC = () => {
  // Form state
  const [name, setName] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  
  // Event settings
  const [triggerEvents, setTriggerEvents] = useState<string[]>(['PickNewVictim']);
  const [murderTypeFilter, setMurderTypeFilter] = useState<string>('TheDoveKiller');
  
  // Item settings
  const [itemToSpawn, setItemToSpawn] = useState<string>('FirstKillLetterInteractable');
  const [spawnChance, setSpawnChance] = useState<number>(1.0);
  
  // Location settings
  const [locationData, setLocationData] = useState({
    locationType: SpawnLocationType.Home,
    roomId: '',
    buildingEntranceType: SubLocationTypeBuildingEntrances.Inside,
    useCustomPosition: false,
    positionX: 0,
    positionY: 0,
    positionZ: 0,
    // Furniture fields
    useFurniture: true,
    furniturePresets: [] as string[],
    // Additional location fields
    randomSpawnLocations: [] as string[],
    hotelRooftopBarSubLocations: [] as string[],
    customRoomNames: [] as string[],
    customRoomPresets: [] as string[],
    customSubRoomNames: [] as string[],
    customSubRoomPresets: [] as string[],
    customFloorNames: [] as string[],
    customRoomName: '',
    customBuildingPreset: ''
  });
  
  // Ownership settings
  const [belongsTo, setBelongsTo] = useState<BelongsTo>(BelongsTo.Murderer);
  const [spawnLocationRecipient, setSpawnLocationRecipient] = useState<BelongsTo>(BelongsTo.Victim);
  
  // Message states
  const [showSuccessMessage, setShowSuccessMessage] = useState<boolean>(false);
  const [messageText, setMessageText] = useState<string>('Configuration saved successfully!');
  const [messageType, setMessageType] = useState<'success' | 'error'>('success');
  
  // Mailbox unlock state
  const [unlockMailbox, setUnlockMailbox] = useState<boolean>(false);

  // Build the rule object
  const buildRule = (): SpawnRule => {
    // Start with the common fields that apply to all location types
    const rule: any = {
      Name: name,
      Enabled: true,
      Description: description || undefined,
      TriggerEvents: triggerEvents,
      MurderMO: murderTypeFilter,
      ItemToSpawn: itemToSpawn,
      SpawnChance: spawnChance,
      SpawnLocation: locationData.locationType,
      // Ownership fields
      BelongsTo: belongsTo,
      Recipient: spawnLocationRecipient,
    };
    
    // Add location-specific fields based on the selected location type
    const locationType = Number(locationData.locationType);
    
    // Add furniture fields if applicable for this location type
    const supportsFurniture = 
      locationType === Number(SpawnLocationType.Home) || 
      locationType === Number(SpawnLocationType.Workplace) || 
      locationType === Number(SpawnLocationType.Custom);
      
    if (supportsFurniture) {
      if (locationData.useFurniture) {
        rule.UseFurniture = true;
        rule.FurniturePresets = locationData.furniturePresets;
      } else {
        rule.UseFurniture = false;
      }
    }
    
    // Add fields specific to each location type
    if (locationType === SpawnLocationType.Random && locationData.randomSpawnLocations?.length > 0) {
      rule.RandomSpawnLocations = locationData.randomSpawnLocations;
    }
    
    if ((locationType === SpawnLocationType.HomeBuildingEntrance || 
         locationType === SpawnLocationType.WorkplaceBuildingEntrance) && 
        locationData.buildingEntranceType !== undefined) {
      rule.SubLocationTypeBuildingEntrances = locationData.buildingEntranceType;
    }
    
    if (locationType === SpawnLocationType.HotelRooftopBar && 
        locationData.hotelRooftopBarSubLocations?.length > 0) {
      rule.HotelRooftopBarSubLocations = locationData.hotelRooftopBarSubLocations;
    }
    
    if (locationType === SpawnLocationType.Custom) {
      // Only add custom fields that have values
      if (locationData.customBuildingPreset) {
        rule.CustomBuildingPreset = locationData.customBuildingPreset;
      }
      
      if (locationData.customRoomNames?.length > 0) {
        rule.CustomRoomNames = locationData.customRoomNames;
      }
      
      if (locationData.customRoomPresets?.length > 0) {
        rule.CustomRoomPresets = locationData.customRoomPresets;
      }
      
      if (locationData.customSubRoomNames?.length > 0) {
        rule.CustomSubRoomNames = locationData.customSubRoomNames;
      }
      
      if (locationData.customSubRoomPresets?.length > 0) {
        rule.CustomSubRoomPresets = locationData.customSubRoomPresets;
      }
      
      if (locationData.customFloorNames?.length > 0) {
        rule.CustomFloorNames = locationData.customFloorNames;
      }
      
      // Custom position fields removed as requested
    }
    
    // Add room name for Home/Workplace
    if ((locationType === SpawnLocationType.Home || 
         locationType === SpawnLocationType.Workplace) && 
        (locationData.customRoomName || locationData.roomId)) {
      rule.CustomRoomName = locationData.customRoomName || locationData.roomId;
    }
    
    // Add mailbox-specific fields
    if (locationType === SpawnLocationType.Mailbox) {
      rule.UnlockMailbox = unlockMailbox;
    }
    
    return rule as SpawnRule;
  };
  
  const handleSaveToLocalStorage = () => {
    const rule = buildRule();
    
    // Create or update the config file
    let configFile: ConfigFile;
    const savedConfig = localStorage.getItem('configFile');
    
    if (savedConfig) {
      configFile = JSON.parse(savedConfig);
      configFile.SpawnRules.push(rule);
    } else {
      configFile = {
        Enabled: true,
        ShowDebugMessages: true,
        SpawnRules: [rule]
      };
    }
    
    localStorage.setItem('configFile', JSON.stringify(configFile, null, 2));
    setShowSuccessMessage(true);
  };
  
  const handleCloseMessage = () => {
    setShowSuccessMessage(false);
  };
  
  // Handle file upload
  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;
    
    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const content = e.target?.result as string;
        const json = JSON.parse(content);
        
        // Check if the JSON has the expected structure
        if (json.SpawnRules && Array.isArray(json.SpawnRules) && json.SpawnRules.length > 0) {
          const rule = json.SpawnRules[0];
          
          // Load the rule data into the form
          setName(rule.Name || '');
          setDescription(rule.Description || '');
          setTriggerEvents(rule.TriggerEvents || ['PickNewVictim']);
          setMurderTypeFilter(rule.MurderMO || 'TheDoveKiller');
          setItemToSpawn(rule.ItemToSpawn || 'FirstKillLetterInteractable');
          setSpawnChance(rule.SpawnChance || 1.0);
          setBelongsTo(rule.BelongsTo || BelongsTo.Murderer);
          setSpawnLocationRecipient(rule.Recipient || BelongsTo.Victim);
          
          // Set unlockMailbox if present
          if (rule.SpawnLocation === SpawnLocationType.Mailbox) {
            setUnlockMailbox(!!rule.UnlockMailbox);
          } else {
            setUnlockMailbox(false);
          }
          
          // Build location data
          const newLocationData: any = {
            locationType: rule.SpawnLocation || SpawnLocationType.Home,
            useFurniture: rule.UseFurniture || false,
            furniturePresets: rule.FurniturePresets || [],
          };
          
          // Add location-specific fields based on the location type
          if (rule.SpawnLocation === SpawnLocationType.HomeBuildingEntrance || 
              rule.SpawnLocation === SpawnLocationType.WorkplaceBuildingEntrance) {
            newLocationData.buildingEntranceType = rule.SubLocationTypeBuildingEntrances;
          }
          
          if (rule.SpawnLocation === SpawnLocationType.Random) {
            newLocationData.randomSpawnLocations = rule.RandomSpawnLocations || [];
          }
          
          if (rule.SpawnLocation === SpawnLocationType.HotelRooftopBar) {
            newLocationData.hotelRooftopBarSubLocations = rule.HotelRooftopBarSubLocations || [];
          }
          
          if (rule.SpawnLocation === SpawnLocationType.Custom) {
            newLocationData.customRoomNames = rule.CustomRoomNames || [];
            newLocationData.customRoomPresets = rule.CustomRoomPresets || [];
            newLocationData.customSubRoomNames = rule.CustomSubRoomNames || [];
            newLocationData.customSubRoomPresets = rule.CustomSubRoomPresets || [];
            newLocationData.customFloorNames = rule.CustomFloorNames || [];
          }
          
          if (rule.SpawnLocation === SpawnLocationType.Home || 
              rule.SpawnLocation === SpawnLocationType.Workplace) {
            newLocationData.roomId = rule.CustomRoomName || '';
            newLocationData.customRoomName = rule.CustomRoomName || '';
          }
          
          setLocationData(newLocationData);
          
          // Show success message
          setMessageText('Configuration loaded successfully!');
          setMessageType('success');
          setShowSuccessMessage(true);
        } else {
          throw new Error('Invalid JSON structure. Expected a SpawnRules array.');
        }
      } catch (error) {
        console.error('Error parsing JSON:', error);
        setMessageText(`Error loading file: ${error instanceof Error ? error.message : 'Invalid JSON format'}`);
        setMessageType('error');
        setShowSuccessMessage(true);
      }
    };
    
    reader.readAsText(file);
    
    // Reset the file input so the same file can be selected again
    event.target.value = '';
  };
  
  const handleClearForm = () => {
    setName('');
    setDescription('');
    setTriggerEvents(['PickNewVictim']);
    setMurderTypeFilter('TheDoveKiller');
    setItemToSpawn('FirstKillLetterInteractable');
    setSpawnChance(1.0);
    setLocationData({
      locationType: SpawnLocationType.Home,
      roomId: '',
      buildingEntranceType: SubLocationTypeBuildingEntrances.Inside,
      useCustomPosition: false,
      positionX: 0,
      positionY: 0,
      positionZ: 0,
      useFurniture: true,
      furniturePresets: ['Table', 'Desk'],
      randomSpawnLocations: [],
      hotelRooftopBarSubLocations: [],
      customRoomNames: [],
      customRoomPresets: [],
      customSubRoomNames: [],
      customSubRoomPresets: [],
      customFloorNames: [],
      customBuildingPreset: '',
      customRoomName: ''
    });
    setBelongsTo(BelongsTo.Murderer);
    setSpawnLocationRecipient(BelongsTo.Victim);
    setUnlockMailbox(false);
  };
  
  return (
    <div className="form-container">
      <Paper elevation={3} sx={{ p: 3, mb: 4, width: '100%', maxWidth: '800px' }}>
        <Typography variant="h4" component="h1" gutterBottom align="center">
          MurderItemSpawner Config Generator
        </Typography>
        <Typography variant="subtitle1" gutterBottom align="center" color="text.secondary">
          Create MIS JSON configurations for your custom cases
        </Typography>
      </Paper>
      
      <Stack spacing={3} sx={{ width: '100%', alignItems: 'center' }}>
        <Paper elevation={2} sx={{ p: 3, width: '100%', maxWidth: '800px' }}>
          <Typography variant="h6" gutterBottom>
            Basic Information
          </Typography>
          <Stack spacing={2}>
            <TextField
              fullWidth
              label="Rule Name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              helperText="A unique name for this spawn rule"
              variant="outlined"
              required
            />
            <TextField
              fullWidth
              label="Description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              helperText="Optional: Describe what this rule does"
              variant="outlined"
              multiline
              rows={2}
            />
          </Stack>
        </Paper>
        
        <Accordion defaultExpanded sx={{ width: '100%', maxWidth: '800px' }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="h6">Event Trigger Settings</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <EventSelector
              triggerEvents={triggerEvents}
              setTriggerEvents={setTriggerEvents}
              murderTypeFilter={murderTypeFilter}
              setMurderTypeFilter={setMurderTypeFilter}
            />
          </AccordionDetails>
        </Accordion>
        
        <Accordion defaultExpanded sx={{ width: '100%', maxWidth: '800px' }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="h6">Item Settings</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <ItemSelector
              itemToSpawn={itemToSpawn}
              setItemToSpawn={setItemToSpawn}
              spawnChance={spawnChance}
              setSpawnChance={setSpawnChance}
            />
          </AccordionDetails>
        </Accordion>
        
        <Accordion defaultExpanded sx={{ width: '100%', maxWidth: '800px' }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="h6">Location Settings</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <LocationSelector
              locationData={locationData}
              onChange={setLocationData}
              unlockMailbox={unlockMailbox}
              setUnlockMailbox={setUnlockMailbox}
            />
          </AccordionDetails>
        </Accordion>
        
        {/* Furniture settings are now handled in the LocationSelector component */}
        
        <Accordion defaultExpanded sx={{ width: '100%', maxWidth: '800px' }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="h6">Ownership Settings</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <OwnershipSelector
              belongsTo={belongsTo}
              setBelongsTo={setBelongsTo}
              spawnLocationRecipient={spawnLocationRecipient}
              setSpawnLocationRecipient={setSpawnLocationRecipient}
            />
          </AccordionDetails>
        </Accordion>
        
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3, gap: 3 }}>
          <Stack direction="row" spacing={2}>
            <Button 
              variant="outlined" 
              color="secondary" 
              onClick={handleClearForm}
            >
              Clear Form
            </Button>
            
            <Button
              variant="outlined"
              component="label"
              color="info"
            >
              Load JSON
              <input
                type="file"
                hidden
                accept=".json"
                onChange={handleFileUpload}
              />
            </Button>
          </Stack>
          
          <Button 
            variant="contained" 
            color="primary" 
            onClick={handleSaveToLocalStorage}
            disabled={!name || !itemToSpawn}
          >
            Save Configuration
          </Button>
        </Box>
        
        <Box sx={{ width: '100%', maxWidth: '800px' }}>
          <Divider sx={{ mb: 3 }} />
          <OutputDisplay rule={buildRule()} />
        </Box>
      </Stack>
      
      <Snackbar
        open={showSuccessMessage}
        autoHideDuration={6000}
        onClose={handleCloseMessage}
      >
        <Alert onClose={handleCloseMessage} severity={messageType} sx={{ width: '100%' }}>
          {messageText}
        </Alert>
      </Snackbar>
    </div>
  );
};

export default ConfigForm;
