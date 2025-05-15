import React from 'react';
import { 
  Typography, 
  Paper, 
  Box, 
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Tab,
  Tabs
} from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import SettingsIcon from '@mui/icons-material/Settings';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import EventIcon from '@mui/icons-material/Event';
import InventoryIcon from '@mui/icons-material/Inventory';
import PersonIcon from '@mui/icons-material/Person';
import CodeIcon from '@mui/icons-material/Code';

import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

// Example JSON configurations
const exampleConfigs = {
  murderWeapon: {
    name: "Murder Weapon at Crime Scene",
    description: "Spawns a knife at the crime scene with the killer's fingerprints",
    json: `{
  "name": "Murder Weapon at Crime Scene",
  "description": "Spawns a knife at the crime scene with the killer's fingerprints",
  "events": ["OnMurderGenerated"],
  "spawnChance": 0.8,
  "itemToSpawn": "Knife",
  "location": {
    "locationType": "MurderRoom",
    "spawnOnFloor": true
  },
  "ownership": {
    "fingerprints": "Killer",
    "recipient": "None"
  }
}`
  },
  incriminatingNote: {
    name: "Incriminating Note at Killer's Home",
    description: "Spawns a note at the killer's home revealing their motive",
    json: `{
  "name": "Incriminating Note at Killer's Home",
  "description": "Spawns a note at the killer's home revealing their motive",
  "events": ["OnMurderGenerated"],
  "spawnChance": 0.6,
  "itemToSpawn": "Murder_Journal",
  "location": {
    "locationType": "Home",
    "spawnOnFurniture": "Desk",
    "recipient": "Killer"
  },
  "ownership": {
    "fingerprints": "Killer",
    "recipient": "Killer"
  }
}`
  },
  victimBelonging: {
    name: "Victim's Belonging at Suspect Location",
    description: "Spawns a victim's belonging at a suspect's workplace",
    json: `{
  "name": "Victim's Belonging at Suspect Location",
  "description": "Spawns a victim's belonging at a suspect's workplace",
  "events": ["OnMurderGenerated"],
  "spawnChance": 0.7,
  "itemToSpawn": "Wallet",
  "location": {
    "locationType": "Workplace",
    "spawnOnFurniture": "FilingCabinet",
    "recipient": "Killer"
  },
  "ownership": {
    "fingerprints": "Victim",
    "recipient": "Victim"
  }
}`
  }
};

const Tutorial: React.FC = () => {
  const [tabValue, setTabValue] = React.useState(0);

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };
  
  return (
    <Box sx={{ width: '100%', maxWidth: '800px', margin: '0 auto', padding: '20px' }}>
      <Paper elevation={3} sx={{ p: 3, mb: 4, width: '100%' }}>
        <Typography variant="h4" component="h1" gutterBottom align="center">
          MurderItemSpawner Basic Tutorial
        </Typography>
        <Typography variant="body1" paragraph>
          Welcome to the MurderItemSpawner Config Generator! This tool helps you create configuration files for the MurderItemSpawner mod for Shadows Of Doubt.
        </Typography>
        <Divider sx={{ my: 3 }} />
        
        <Typography variant="h5" gutterBottom>
          Getting Started
        </Typography>
        <Typography variant="body1" paragraph>
          The MurderItemSpawner mod allows you to configure custom item spawns that trigger during specific game events. This tool makes it easy to create and manage these spawn configurations.
        </Typography>
        
        <List>
          <ListItem>
            <ListItemIcon>
              <InfoIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Basic Information" 
              secondary="Start by giving your spawn rule a unique name and description. This helps you identify the rule later." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <EventIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Event Trigger Settings" 
              secondary="Choose which game events will trigger your item spawn. You can select multiple events if needed." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <InventoryIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Item Settings" 
              secondary="Select which item to spawn and set the spawn chance (0.0 to 1.0, where 1.0 is 100%)." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <LocationOnIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Location Settings" 
              secondary="Define where the item should spawn. Options include specific rooms, random locations, or furniture." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <PersonIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Ownership Settings" 
              secondary="Determine who the item belongs to (fingerprints) and who the recipient is (In cases such as home, if you make the recipient the victim, it will spawn at their home)." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <CodeIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="JSON Output" 
              secondary="After configuring all settings, the tool generates a JSON configuration that you can save and use with the mod." 
            />
          </ListItem>
        </List>
        
        <Divider sx={{ my: 3 }} />
        
        <Typography variant="h5" gutterBottom>
          Advanced Usage
        </Typography>
        <Typography variant="body1" paragraph>
          For advanced users, you can:
        </Typography>
        <List>
          <ListItem>
            <ListItemIcon>
              <SettingsIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Load Existing Configurations" 
              secondary="Use the 'Load JSON' button to load and edit existing configuration files." 
            />
          </ListItem>
        </List>
        
        <Box sx={{ mt: 4 }}>
          <Typography variant="h5" gutterBottom>
            Example Use Cases
          </Typography>
          <Typography variant="body1" paragraph>
            Here are some common use cases with example configurations:
          </Typography>
          
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs value={tabValue} onChange={handleTabChange} aria-label="example configurations">
              <Tab label="Murder Weapon" />
              <Tab label="Incriminating Note" />
              <Tab label="Victim's Belonging" />
            </Tabs>
          </Box>
          
          <TabPanel value={tabValue} index={0}>
            <Typography variant="h6">{exampleConfigs.murderWeapon.name}</Typography>
            <Typography variant="body2" paragraph>{exampleConfigs.murderWeapon.description}</Typography>
            <Box>
              <SyntaxHighlighter language="json" style={vscDarkPlus} wrapLongLines>
                {exampleConfigs.murderWeapon.json}
              </SyntaxHighlighter>
            </Box>
          </TabPanel>
          
          <TabPanel value={tabValue} index={1}>
            <Typography variant="h6">{exampleConfigs.incriminatingNote.name}</Typography>
            <Typography variant="body2" paragraph>{exampleConfigs.incriminatingNote.description}</Typography>
            <Box>
              <SyntaxHighlighter language="json" style={vscDarkPlus} wrapLongLines>
                {exampleConfigs.incriminatingNote.json}
              </SyntaxHighlighter>
            </Box>
          </TabPanel>
          
          <TabPanel value={tabValue} index={2}>
            <Typography variant="h6">{exampleConfigs.victimBelonging.name}</Typography>
            <Typography variant="body2" paragraph>{exampleConfigs.victimBelonging.description}</Typography>
            <Box>
              <SyntaxHighlighter language="json" style={vscDarkPlus} wrapLongLines>
                {exampleConfigs.victimBelonging.json}
              </SyntaxHighlighter>
            </Box>
          </TabPanel>
        </Box>
        
        <Divider sx={{ my: 3 }} />
        
        <Typography variant="h5" gutterBottom>
          Installation
        </Typography>
        <Typography variant="body1" paragraph>
          To use the configurations created with this tool:
        </Typography>
        <Typography variant="body1" component="ol" sx={{ pl: 2 }}>
          <li>Save the generated JSON configuration</li>
          <li>Place the JSON file in the mod's plugin folder</li>
          <li>Reload a save game (you do not need to restart the game as long as the mod is already installed) </li>
        </Typography>
        
        <Box sx={{ mt: 4, p: 2, bgcolor: 'rgba(0, 0, 0, 0.04)', borderRadius: 1 }}>
          <Typography variant="body2" color="textSecondary">
            Note: This tool is designed for use with the MurderItemSpawner mod for Shadows Of Doubt. 
            For more information about the mod itself, please refer to the mod documentation.
          </Typography>
        </Box>
      </Paper>
    </Box>
  );
};

export default Tutorial;