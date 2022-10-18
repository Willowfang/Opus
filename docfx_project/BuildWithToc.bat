:: Create metadata for modification
docfx metadata
:: Reshape TOC with namespace nesting
node namespaces.js
:: Build site and serve it
docfx build --serve