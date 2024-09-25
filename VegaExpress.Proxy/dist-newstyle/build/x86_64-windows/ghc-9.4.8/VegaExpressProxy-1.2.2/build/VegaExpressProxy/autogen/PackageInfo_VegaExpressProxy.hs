{-# LANGUAGE NoRebindableSyntax #-}
{-# OPTIONS_GHC -fno-warn-missing-import-lists #-}
{-# OPTIONS_GHC -w #-}
module PackageInfo_VegaExpressProxy (
    name,
    version,
    synopsis,
    copyright,
    homepage,
  ) where

import Data.Version (Version(..))
import Prelude

name :: String
name = "VegaExpressProxy"
version :: Version
version = Version [1,2,2] []

synopsis :: String
synopsis = "High-performance Distributed Proxy and Tunneling Solution for TCP"
copyright :: String
copyright = "Copyright \169 2024 Liiksoft"
homepage :: String
homepage = ""
