export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  baseCurrency: 'EUR' | 'USD' | 'RON';
  isActive: boolean;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: UserProfile;
}

export interface Portfolio {
  id: number;
  name: string;
  description?: string;
  createdDate: string;
}

export interface Holding {
  id: number;
  portfolioId: number;
  symbol: string;
  quantity: number;
  averagePurchasePrice: number;
  currency: 'EUR' | 'USD' | 'RON';
  purchaseDate: string;
  groupId: number;
  groupName: string;
}

export interface HoldingGroup {
  id: number;
  name: string;
  description?: string;
  createdAtUtc: string;
}

export interface Quote {
  symbol: string;
  price: number;
  high52Week?: number;
  low52Week?: number;
  companyName: string;
  marketCap?: number;
  currency: string;
  retrievedAtUtc: string;
}

export interface PositionAnalytics {
  holdingId: number;
  symbol: string;
  holdingCurrency: string;
  groupName: string;
  quantity: number;
  averagePurchasePrice: number;
  currentPrice: number;
  marketValueBase: number;
  costBasisBase: number;
  unrealizedPnLBase: number;
  unrealizedPnLPercent: number;
}

export interface GroupAllocation {
  groupId: number;
  groupName: string;
  marketValueBase: number;
  allocationPercent: number;
}

export interface CurrencyExposure {
  currency: string;
  marketValueBase: number;
  allocationPercent: number;
}

export interface PortfolioAnalytics {
  portfolioId: number;
  baseCurrency: string;
  totalMarketValueBase: number;
  totalCostBasisBase: number;
  totalUnrealizedPnLBase: number;
  totalUnrealizedPnLPercent: number;
  isPartial: boolean;
  missingSymbolsCount: number;
  positions: PositionAnalytics[];
  groupAllocations: GroupAllocation[];
  currencyExposures: CurrencyExposure[];
}

export interface Snapshot {
  id: number;
  portfolioId: number;
  capturedAtUtc: string;
  baseCurrency: string;
  totalMarketValueBase: number;
  totalCostBasisBase: number;
  totalUnrealizedPnLBase: number;
  isPartial: boolean;
  missingSymbolsCount: number;
  fxTimestampUtc: string;
  eurUsdRate: number;
  eurRonRate: number;
}

export interface SnapshotCreateResponse {
  snapshot: Snapshot;
}

export interface TimeSeriesPoint {
  capturedAtUtc: string;
  totalMarketValueBase: number;
  totalCostBasisBase: number;
  totalUnrealizedPnLBase: number;
  isPartial: boolean;
}
