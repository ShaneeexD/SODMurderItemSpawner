// Using simple objects instead of enums for better compatibility with erasableSyntaxOnly

// SpawnLocationType values
export const SpawnLocationType = {
  Mailbox: 0,
  Doormat: 1,
  HomeLobby: 2,
  WorkplaceLobby: 3,
  HomeBuildingEntrance: 4,
  WorkplaceBuildingEntrance: 5,
  CityHallBathroom: 6,
  HotelRooftopBar: 7,
  Home: 8,
  Workplace: 9,
  Random: 10,
  Custom: 11
} as const;

// Type for SpawnLocationType
export type SpawnLocationType = (typeof SpawnLocationType)[keyof typeof SpawnLocationType];

// BelongsTo values
export const BelongsTo = {
  Murderer: 0,
  Victim: 1,
  Player: 2,
  MurdererDoctor: 3,
  VictimDoctor: 4,
  MurdererLandlord: 5,
  VictimLandlord: 6
} as const;

// Type for BelongsTo
export type BelongsTo = (typeof BelongsTo)[keyof typeof BelongsTo];

// SubLocationTypeBuildingEntrances values
export const SubLocationTypeBuildingEntrances = {
  Inside: 0,
  Outside: 1
} as const;

// Type for SubLocationTypeBuildingEntrances
export type SubLocationTypeBuildingEntrances = (typeof SubLocationTypeBuildingEntrances)[keyof typeof SubLocationTypeBuildingEntrances];

export interface Vector3 {
  X: number;
  Y: number;
  Z: number;
}

export interface SpawnRule {
  Name: string;
  Enabled: boolean;
  Description?: string;
  TriggerEvents: string[];
  MurderMO?: string;
  ItemToSpawn: string;
  SpawnChance: number;
  SpawnLocation: SpawnLocationType;
  RandomSpawnLocations?: string[];
  SubLocationTypeBuildingEntrances?: SubLocationTypeBuildingEntrances;
  HotelRooftopBarSubLocations?: string[];
  CustomBuildingPreset?: string;
  CustomRoomNames?: string[];
  CustomRoomPresets?: string[];
  CustomSubRoomNames?: string[];
  CustomSubRoomPresets?: string[];
  CustomFloorNames?: string[];
  CustomRoomName?: string;
  UseFurniture: boolean;
  FurniturePresets?: string[];
  BelongsTo: BelongsTo;
  Recipient: BelongsTo;
  CustomPosition?: Vector3;
  UnlockMailbox?: boolean;
}

export interface ConfigFile {
  Enabled: boolean;
  ShowDebugMessages: boolean;
  SpawnRules: SpawnRule[];
}
